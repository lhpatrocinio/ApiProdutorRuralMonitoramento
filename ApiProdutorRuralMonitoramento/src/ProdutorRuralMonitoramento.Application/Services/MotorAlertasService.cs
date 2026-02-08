using Microsoft.Extensions.Logging;
using ProdutorRuralMonitoramento.Application.Services.Interfaces;
using ProdutorRuralMonitoramento.Domain.Entities;
using ProdutorRuralMonitoramento.Domain.Interfaces;

namespace ProdutorRuralMonitoramento.Application.Services;

/// <summary>
/// Motor de processamento de alertas - avalia leituras contra regras configuradas
/// </summary>
public class MotorAlertasService : IMotorAlertasService
{
    private readonly IRegraAlertaRepository _regraAlertaRepository;
    private readonly IAlertaRepository _alertaRepository;
    private readonly IHistoricoStatusTalhaoRepository _historicoRepository;
    private readonly IAlertaEventPublisher _eventPublisher;
    private readonly ILogger<MotorAlertasService> _logger;

    public MotorAlertasService(
        IRegraAlertaRepository regraAlertaRepository,
        IAlertaRepository alertaRepository,
        IHistoricoStatusTalhaoRepository historicoRepository,
        IAlertaEventPublisher eventPublisher,
        ILogger<MotorAlertasService> logger)
    {
        _regraAlertaRepository = regraAlertaRepository;
        _alertaRepository = alertaRepository;
        _historicoRepository = historicoRepository;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    /// <summary>
    /// Processa uma leitura de sensor recebida via RabbitMQ
    /// </summary>
    public async Task ProcessarLeituraAsync(SensorDataEvent leitura)
    {
        _logger.LogInformation(
            "Processando leitura do sensor {SensorId} no talhão {TalhaoId}: {TipoLeitura} = {Valor}",
            leitura.SensorId, leitura.TalhaoId, leitura.TipoLeitura, leitura.Valor);

        try
        {
            // Buscar regras ativas para o talhão ou globais do produtor
            var regrasAtivas = await _regraAlertaRepository.GetAtivasByTalhaoIdAsync(leitura.TalhaoId);
            
            if (!regrasAtivas.Any())
            {
                _logger.LogDebug("Nenhuma regra ativa encontrada para o talhão {TalhaoId}", leitura.TalhaoId);
                return;
            }

            // Filtrar regras relevantes para o campo da leitura
            var regrasRelevantes = regrasAtivas
                .Where(r => r.Campo.Equals(leitura.TipoLeitura, StringComparison.OrdinalIgnoreCase))
                .ToList();

            _logger.LogDebug("Encontradas {Count} regras relevantes para {TipoLeitura}", 
                regrasRelevantes.Count, leitura.TipoLeitura);

            foreach (var regra in regrasRelevantes)
            {
                if (AvaliarRegra(regra, leitura.Valor))
                {
                    await CriarAlertaAsync(regra, leitura);
                }
            }

            // Atualizar histórico de status do talhão
            await AtualizarHistoricoTalhaoAsync(leitura);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar leitura do sensor {SensorId}", leitura.SensorId);
            throw;
        }
    }

    /// <summary>
    /// Avalia se uma regra foi violada dado um valor de leitura
    /// </summary>
    private bool AvaliarRegra(RegraAlerta regra, decimal valor)
    {
        var resultado = regra.Operador switch
        {
            Operador.Igual => valor == regra.Valor,
            Operador.Diferente => valor != regra.Valor,
            Operador.Maior => valor > regra.Valor,
            Operador.MaiorOuIgual => valor >= regra.Valor,
            Operador.Menor => valor < regra.Valor,
            Operador.MenorOuIgual => valor <= regra.Valor,
            Operador.Entre => false, // Não implementado nesta versão
            Operador.ForaDe => false, // Não implementado nesta versão
            _ => false
        };

        if (resultado)
        {
            _logger.LogInformation(
                "Regra '{RegraName}' violada: {Valor} {Operador} {RegraValor}",
                regra.Nome, valor, regra.Operador, regra.Valor);
        }

        return resultado;
    }

    /// <summary>
    /// Cria um novo alerta e publica evento via RabbitMQ
    /// </summary>
    private async Task CriarAlertaAsync(RegraAlerta regra, SensorDataEvent leitura)
    {
        // Verificar se já existe alerta não resolvido para esta regra/talhão
        var alertaExistente = await _alertaRepository
            .GetNaoResolvidoByRegraETalhaoAsync(regra.Id, leitura.TalhaoId);

        if (alertaExistente != null)
        {
            _logger.LogDebug(
                "Alerta já existe para regra {RegraId} no talhão {TalhaoId}, não criando duplicado",
                regra.Id, leitura.TalhaoId);
            return;
        }

        var mensagem = GerarMensagemAlerta(regra, leitura);
        
        var alerta = new Alerta(
            produtorId: regra.ProdutorId,
            talhaoId: leitura.TalhaoId,
            regraAlertaId: regra.Id,
            tipoAlerta: regra.TipoAlerta,
            severidade: regra.Severidade,
            titulo: $"Alerta: {regra.Nome}",
            mensagem: mensagem,
            valorLeitura: leitura.Valor,
            leituraId: leitura.LeituraId
        );

        await _alertaRepository.AddAsync(alerta);

        _logger.LogInformation(
            "Alerta {AlertaId} criado - Tipo: {TipoAlerta}, Severidade: {Severidade}",
            alerta.Id, alerta.TipoAlerta, alerta.Severidade);

        // Publicar evento para API Cadastro atualizar o produtor
        try
        {
            await _eventPublisher.PublishAlertaCriadoAsync(alerta);
            _logger.LogInformation("Evento AlertCreated publicado para alerta {AlertaId}", alerta.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao publicar evento AlertCreated para alerta {AlertaId}", alerta.Id);
            // Não relançar - o alerta já foi criado, o evento pode ser republicado depois
        }
    }

    /// <summary>
    /// Gera uma mensagem descritiva para o alerta
    /// </summary>
    private static string GerarMensagemAlerta(RegraAlerta regra, SensorDataEvent leitura)
    {
        var operadorDescricao = regra.Operador switch
        {
            Operador.Igual => "igual a",
            Operador.Diferente => "diferente de",
            Operador.Maior => "acima de",
            Operador.MaiorOuIgual => "igual ou acima de",
            Operador.Menor => "abaixo de",
            Operador.MenorOuIgual => "igual ou abaixo de",
            _ => regra.Operador.ToString()
        };

        var unidade = leitura.TipoLeitura.ToLower() switch
        {
            "temperatura" => "°C",
            "umidade" => "%",
            "umidadesolo" or "umidade_solo" => "%",
            "precipitacao" => "mm",
            "velocidadevento" or "velocidade_vento" => "km/h",
            "radiacao" or "radiacaosolar" => "W/m²",
            _ => ""
        };

        return $"Leitura de {leitura.TipoLeitura} registrou valor de {leitura.Valor}{unidade}, " +
               $"que está {operadorDescricao} o limite configurado de {regra.Valor}{unidade}. " +
               $"Regra: {regra.Descricao ?? regra.Nome}";
    }

    /// <summary>
    /// Atualiza o histórico de status do talhão com base na leitura
    /// </summary>
    private async Task AtualizarHistoricoTalhaoAsync(SensorDataEvent leitura)
    {
        try
        {
            // Determinar o status com base nos valores da leitura
            var status = DeterminarStatusTalhao(leitura);
            
            // Verificar se já existe histórico com mesmo status
            var ultimoHistorico = await _historicoRepository.GetUltimoByTalhaoIdAsync(leitura.TalhaoId);
            
            if (ultimoHistorico?.Status == status)
            {
                // Mesmo status, não criar novo registro
                return;
            }

            var historico = new HistoricoStatusTalhao(
                talhaoId: leitura.TalhaoId,
                status: status,
                descricao: $"Status atualizado com base em leitura de {leitura.TipoLeitura}: {leitura.Valor}",
                leituraId: leitura.LeituraId
            );

            await _historicoRepository.AddAsync(historico);
            
            _logger.LogDebug("Histórico de status atualizado para talhão {TalhaoId}: {Status}", 
                leitura.TalhaoId, status);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erro ao atualizar histórico de status do talhão {TalhaoId}", leitura.TalhaoId);
            // Não relançar - é uma atualização secundária
        }
    }

    /// <summary>
    /// Determina o status do talhão com base na leitura
    /// </summary>
    private static string DeterminarStatusTalhao(SensorDataEvent leitura)
    {
        // Lógica simplificada para determinar status
        var tipoLeitura = leitura.TipoLeitura.ToLower();
        var valor = leitura.Valor;

        return tipoLeitura switch
        {
            "temperatura" when valor > 35 => "Temperatura Alta",
            "temperatura" when valor < 5 => "Temperatura Baixa",
            "umidadesolo" or "umidade_solo" when valor < 20 => "Solo Seco",
            "umidadesolo" or "umidade_solo" when valor > 80 => "Solo Encharcado",
            "precipitacao" when valor > 50 => "Chuva Intensa",
            _ => "Normal"
        };
    }
}
