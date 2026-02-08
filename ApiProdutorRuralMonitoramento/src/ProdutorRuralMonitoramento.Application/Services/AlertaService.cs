using AutoMapper;
using Microsoft.Extensions.Logging;
using ProdutorRuralMonitoramento.Application.DTOs.Request;
using ProdutorRuralMonitoramento.Application.DTOs.Response;
using ProdutorRuralMonitoramento.Application.Services.Interfaces;
using ProdutorRuralMonitoramento.Domain.Entities;
using ProdutorRuralMonitoramento.Domain.Interfaces;

namespace ProdutorRuralMonitoramento.Application.Services;

/// <summary>
/// Implementação do serviço de Alertas
/// </summary>
public class AlertaService : IAlertaService
{
    private readonly IAlertaRepository _alertaRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<AlertaService> _logger;

    public AlertaService(
        IAlertaRepository alertaRepository,
        IMapper mapper,
        ILogger<AlertaService> logger)
    {
        _alertaRepository = alertaRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<AlertaResponse>> GetAllByProdutorAsync(Guid produtorId)
    {
        var alertas = await _alertaRepository.GetByProdutorIdAsync(produtorId);
        return _mapper.Map<IEnumerable<AlertaResponse>>(alertas);
    }

    public async Task<AlertaComRegraResponse?> GetByIdAsync(Guid id)
    {
        var alerta = await _alertaRepository.GetByIdAsync(id);
        if (alerta == null) return null;
        return _mapper.Map<AlertaComRegraResponse>(alerta);
    }

    public async Task<IEnumerable<AlertaResponse>> GetByTalhaoAsync(Guid talhaoId)
    {
        var alertas = await _alertaRepository.GetByTalhaoIdAsync(talhaoId);
        return _mapper.Map<IEnumerable<AlertaResponse>>(alertas);
    }

    public async Task<IEnumerable<AlertaResponse>> GetNaoLidosAsync(Guid produtorId)
    {
        var alertas = await _alertaRepository.GetNaoLidosAsync(produtorId);
        return _mapper.Map<IEnumerable<AlertaResponse>>(alertas);
    }

    public async Task<IEnumerable<AlertaResponse>> GetNaoResolvidosAsync(Guid produtorId)
    {
        var alertas = await _alertaRepository.GetNaoResolvidosAsync(produtorId);
        return _mapper.Map<IEnumerable<AlertaResponse>>(alertas);
    }

    public async Task<IEnumerable<AlertaResponse>> GetByFiltroAsync(Guid produtorId, AlertaFiltroRequest filtro)
    {
        var alertas = await _alertaRepository.GetByProdutorIdAsync(produtorId);
        
        // Aplicar filtros
        if (filtro.TalhaoId.HasValue)
            alertas = alertas.Where(a => a.TalhaoId == filtro.TalhaoId.Value);
        
        if (filtro.TipoAlerta.HasValue)
            alertas = alertas.Where(a => a.TipoAlerta == filtro.TipoAlerta.Value);
        
        if (filtro.Severidade.HasValue)
            alertas = alertas.Where(a => a.Severidade == filtro.Severidade.Value);
        
        if (filtro.ApenasNaoLidos == true)
            alertas = alertas.Where(a => !a.Lido);
        
        if (filtro.ApenasNaoResolvidos == true)
            alertas = alertas.Where(a => !a.Resolvido);
        
        if (filtro.DataInicio.HasValue)
            alertas = alertas.Where(a => a.CreatedAt >= filtro.DataInicio.Value);
        
        if (filtro.DataFim.HasValue)
            alertas = alertas.Where(a => a.CreatedAt <= filtro.DataFim.Value);
        
        return _mapper.Map<IEnumerable<AlertaResponse>>(alertas);
    }

    public async Task<IEnumerable<AlertaResponse>> GetRecentesAsync(Guid produtorId, int quantidade = 10)
    {
        var alertas = await _alertaRepository.GetRecentesAsync(produtorId, quantidade);
        return _mapper.Map<IEnumerable<AlertaResponse>>(alertas);
    }

    public async Task<AlertaResumoResponse> GetResumoAsync(Guid produtorId)
    {
        var alertas = await _alertaRepository.GetByProdutorIdAsync(produtorId);
        var lista = alertas.ToList();
        
        return new AlertaResumoResponse
        {
            ProdutorId = produtorId,
            TotalAlertas = lista.Count,
            AlertasNaoLidos = lista.Count(a => !a.Lido),
            AlertasNaoResolvidos = lista.Count(a => !a.Resolvido),
            AlertasCriticos = lista.Count(a => a.Severidade == Severidade.Critica && !a.Resolvido),
            AlertasAltos = lista.Count(a => a.Severidade == Severidade.Alta && !a.Resolvido),
            AlertasMedios = lista.Count(a => a.Severidade == Severidade.Media && !a.Resolvido),
            AlertasBaixos = lista.Count(a => a.Severidade == Severidade.Baixa && !a.Resolvido),
            AlertasSeca = lista.Count(a => a.TipoAlerta == TipoAlerta.Seca && !a.Resolvido),
            AlertasTemperatura = lista.Count(a => a.TipoAlerta == TipoAlerta.Temperatura && !a.Resolvido),
            AlertasPrecipitacao = lista.Count(a => a.TipoAlerta == TipoAlerta.Precipitacao && !a.Resolvido),
            AlertasGeada = lista.Count(a => a.TipoAlerta == TipoAlerta.Geada && !a.Resolvido),
            UltimoAlerta = lista.OrderByDescending(a => a.CreatedAt).FirstOrDefault()?.CreatedAt
        };
    }

    public async Task<AlertaResponse> MarcarComoLidoAsync(Guid alertaId)
    {
        var alerta = await _alertaRepository.GetByIdAsync(alertaId);
        if (alerta == null)
            throw new KeyNotFoundException($"Alerta {alertaId} não encontrado");
        
        alerta.MarcarComoLido();
        await _alertaRepository.UpdateAsync(alerta);
        
        _logger.LogInformation("Alerta {AlertaId} marcado como lido", alertaId);
        
        return _mapper.Map<AlertaResponse>(alerta);
    }

    public async Task MarcarTodosComoLidosAsync(Guid produtorId)
    {
        await _alertaRepository.MarcarTodosComoLidosAsync(produtorId);
        _logger.LogInformation("Todos alertas do produtor {ProdutorId} marcados como lidos", produtorId);
    }

    public async Task<AlertaResponse> ResolverAsync(Guid alertaId, Guid resolvidoPor, AlertaResolverRequest request)
    {
        var alerta = await _alertaRepository.GetByIdAsync(alertaId);
        if (alerta == null)
            throw new KeyNotFoundException($"Alerta {alertaId} não encontrado");
        
        alerta.Resolver(resolvidoPor, request.Observacao);
        await _alertaRepository.UpdateAsync(alerta);
        
        _logger.LogInformation("Alerta {AlertaId} resolvido por {ResolvidoPor}", alertaId, resolvidoPor);
        
        return _mapper.Map<AlertaResponse>(alerta);
    }

    public async Task<int> CountNaoLidosAsync(Guid produtorId)
    {
        return await _alertaRepository.CountNaoLidosAsync(produtorId);
    }

    public async Task<int> CountNaoResolvidosAsync(Guid produtorId)
    {
        return await _alertaRepository.CountNaoResolvidosAsync(produtorId);
    }
}
