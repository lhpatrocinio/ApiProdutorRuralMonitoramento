using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using ProdutorRuralMonitoramento.Application.Services;
using ProdutorRuralMonitoramento.Application.Services.Interfaces;
using ProdutorRuralMonitoramento.Domain.Entities;
using ProdutorRuralMonitoramento.Domain.Interfaces;

namespace ProdutorRuralMonitoramento.Tests.Services;

public class MotorAlertasServiceTests
{
    private readonly Mock<IRegraAlertaRepository> _regraRepoMock;
    private readonly Mock<IAlertaRepository> _alertaRepoMock;
    private readonly Mock<IHistoricoStatusTalhaoRepository> _historicoRepoMock;
    private readonly Mock<IAlertaEventPublisher> _eventPublisherMock;
    private readonly Mock<ILogger<MotorAlertasService>> _loggerMock;
    private readonly MotorAlertasService _service;

    public MotorAlertasServiceTests()
    {
        _regraRepoMock = new Mock<IRegraAlertaRepository>();
        _alertaRepoMock = new Mock<IAlertaRepository>();
        _historicoRepoMock = new Mock<IHistoricoStatusTalhaoRepository>();
        _eventPublisherMock = new Mock<IAlertaEventPublisher>();
        _loggerMock = new Mock<ILogger<MotorAlertasService>>();
        _service = new MotorAlertasService(
            _regraRepoMock.Object,
            _alertaRepoMock.Object,
            _historicoRepoMock.Object,
            _eventPublisherMock.Object,
            _loggerMock.Object);
    }

    private static SensorDataEvent CriarLeitura(
        decimal? umidadeSolo = null, decimal? temperatura = null,
        decimal? precipitacao = null, decimal? umidadeAr = null,
        decimal? velocidadeVento = null, decimal? radiacaoSolar = null)
    {
        return new SensorDataEvent
        {
            EventId = Guid.NewGuid(),
            EventDateTime = DateTime.UtcNow,
            LeituraId = Guid.NewGuid(),
            TalhaoId = Guid.NewGuid(),
            SensorId = Guid.NewGuid(),
            CodigoSensor = "SENS-001",
            UmidadeSolo = umidadeSolo,
            Temperatura = temperatura,
            Precipitacao = precipitacao,
            UmidadeAr = umidadeAr,
            VelocidadeVento = velocidadeVento,
            RadiacaoSolar = radiacaoSolar,
            DataHoraLeitura = DateTime.UtcNow
        };
    }

    private static RegraAlerta CriarRegra(
        string campo, Operador operador, decimal valor,
        TipoAlerta tipo = TipoAlerta.Temperatura,
        Severidade severidade = Severidade.Alta)
    {
        return new RegraAlerta(
            produtorId: Guid.NewGuid(),
            talhaoId: Guid.NewGuid(),
            nome: $"Regra {campo}",
            descricao: $"Teste {campo}",
            campo: campo,
            operador: operador,
            valor: valor,
            tipoAlerta: tipo,
            severidade: severidade);
    }

    [Fact]
    public async Task ProcessarLeituraAsync_SemRegrasAtivas_NaoDeveCriarAlerta()
    {
        var leitura = CriarLeitura(temperatura: 50m);
        _regraRepoMock.Setup(r => r.GetAtivasByTalhaoIdAsync(leitura.TalhaoId))
            .ReturnsAsync(Enumerable.Empty<RegraAlerta>());
        _historicoRepoMock.Setup(r => r.GetUltimoByTalhaoIdAsync(leitura.TalhaoId))
            .ReturnsAsync((HistoricoStatusTalhao?)null);
        _historicoRepoMock.Setup(r => r.AddAsync(It.IsAny<HistoricoStatusTalhao>()))
            .ReturnsAsync((HistoricoStatusTalhao h) => h);

        await _service.ProcessarLeituraAsync(leitura);

        _alertaRepoMock.Verify(r => r.AddAsync(It.IsAny<Alerta>()), Times.Never);
    }

    [Fact]
    public async Task ProcessarLeituraAsync_ComRegraViolada_DeveCriarAlerta()
    {
        var leitura = CriarLeitura(temperatura: 42m);
        var regra = CriarRegra("temperatura", Operador.Maior, 40m);

        _regraRepoMock.Setup(r => r.GetAtivasByTalhaoIdAsync(leitura.TalhaoId))
            .ReturnsAsync(new[] { regra });
        _alertaRepoMock.Setup(r => r.GetNaoResolvidoByRegraETalhaoAsync(regra.Id, leitura.TalhaoId))
            .ReturnsAsync((Alerta?)null);
        _alertaRepoMock.Setup(r => r.AddAsync(It.IsAny<Alerta>()))
            .ReturnsAsync((Alerta a) => a);
        _eventPublisherMock.Setup(p => p.PublishAlertaCriadoAsync(It.IsAny<Alerta>()))
            .Returns(Task.CompletedTask);
        _historicoRepoMock.Setup(r => r.GetUltimoByTalhaoIdAsync(leitura.TalhaoId))
            .ReturnsAsync((HistoricoStatusTalhao?)null);
        _historicoRepoMock.Setup(r => r.AddAsync(It.IsAny<HistoricoStatusTalhao>()))
            .ReturnsAsync((HistoricoStatusTalhao h) => h);

        await _service.ProcessarLeituraAsync(leitura);

        _alertaRepoMock.Verify(r => r.AddAsync(It.IsAny<Alerta>()), Times.Once);
        _eventPublisherMock.Verify(p => p.PublishAlertaCriadoAsync(It.IsAny<Alerta>()), Times.Once);
    }

    [Fact]
    public async Task ProcessarLeituraAsync_ComRegraNaoViolada_NaoDeveCriarAlerta()
    {
        var leitura = CriarLeitura(temperatura: 25m);
        var regra = CriarRegra("temperatura", Operador.Maior, 40m);

        _regraRepoMock.Setup(r => r.GetAtivasByTalhaoIdAsync(leitura.TalhaoId))
            .ReturnsAsync(new[] { regra });
        _historicoRepoMock.Setup(r => r.GetUltimoByTalhaoIdAsync(leitura.TalhaoId))
            .ReturnsAsync((HistoricoStatusTalhao?)null);
        _historicoRepoMock.Setup(r => r.AddAsync(It.IsAny<HistoricoStatusTalhao>()))
            .ReturnsAsync((HistoricoStatusTalhao h) => h);

        await _service.ProcessarLeituraAsync(leitura);

        _alertaRepoMock.Verify(r => r.AddAsync(It.IsAny<Alerta>()), Times.Never);
    }

    [Fact]
    public async Task ProcessarLeituraAsync_ComAlertaExistente_NaoDeveDuplicar()
    {
        var leitura = CriarLeitura(temperatura: 42m);
        var regra = CriarRegra("temperatura", Operador.Maior, 40m);
        var alertaExistente = new Alerta(Guid.NewGuid(), leitura.TalhaoId, regra.Id,
            TipoAlerta.Temperatura, Severidade.Alta, "Existente", "msg");

        _regraRepoMock.Setup(r => r.GetAtivasByTalhaoIdAsync(leitura.TalhaoId))
            .ReturnsAsync(new[] { regra });
        _alertaRepoMock.Setup(r => r.GetNaoResolvidoByRegraETalhaoAsync(regra.Id, leitura.TalhaoId))
            .ReturnsAsync(alertaExistente);
        _historicoRepoMock.Setup(r => r.GetUltimoByTalhaoIdAsync(leitura.TalhaoId))
            .ReturnsAsync((HistoricoStatusTalhao?)null);
        _historicoRepoMock.Setup(r => r.AddAsync(It.IsAny<HistoricoStatusTalhao>()))
            .ReturnsAsync((HistoricoStatusTalhao h) => h);

        await _service.ProcessarLeituraAsync(leitura);

        _alertaRepoMock.Verify(r => r.AddAsync(It.IsAny<Alerta>()), Times.Never);
    }

    [Theory]
    [InlineData(Operador.Maior, 30, 40, true)]
    [InlineData(Operador.Maior, 30, 25, false)]
    [InlineData(Operador.Menor, 30, 25, true)]
    [InlineData(Operador.Menor, 30, 35, false)]
    [InlineData(Operador.MaiorOuIgual, 30, 30, true)]
    [InlineData(Operador.MaiorOuIgual, 30, 29, false)]
    [InlineData(Operador.MenorOuIgual, 30, 30, true)]
    [InlineData(Operador.MenorOuIgual, 30, 31, false)]
    [InlineData(Operador.Igual, 30, 30, true)]
    [InlineData(Operador.Igual, 30, 31, false)]
    [InlineData(Operador.Diferente, 30, 31, true)]
    [InlineData(Operador.Diferente, 30, 30, false)]
    public async Task ProcessarLeituraAsync_Operadores_DeveAvaliarCorretamente(
        Operador operador, decimal valorRegra, decimal valorLeitura, bool deveAlertar)
    {
        var leitura = CriarLeitura(temperatura: valorLeitura);
        var regra = CriarRegra("temperatura", operador, valorRegra);

        _regraRepoMock.Setup(r => r.GetAtivasByTalhaoIdAsync(leitura.TalhaoId))
            .ReturnsAsync(new[] { regra });
        _alertaRepoMock.Setup(r => r.GetNaoResolvidoByRegraETalhaoAsync(regra.Id, leitura.TalhaoId))
            .ReturnsAsync((Alerta?)null);
        _alertaRepoMock.Setup(r => r.AddAsync(It.IsAny<Alerta>()))
            .ReturnsAsync((Alerta a) => a);
        _eventPublisherMock.Setup(p => p.PublishAlertaCriadoAsync(It.IsAny<Alerta>()))
            .Returns(Task.CompletedTask);
        _historicoRepoMock.Setup(r => r.GetUltimoByTalhaoIdAsync(leitura.TalhaoId))
            .ReturnsAsync((HistoricoStatusTalhao?)null);
        _historicoRepoMock.Setup(r => r.AddAsync(It.IsAny<HistoricoStatusTalhao>()))
            .ReturnsAsync((HistoricoStatusTalhao h) => h);

        await _service.ProcessarLeituraAsync(leitura);

        _alertaRepoMock.Verify(
            r => r.AddAsync(It.IsAny<Alerta>()),
            deveAlertar ? Times.Once() : Times.Never());
    }

    [Fact]
    public async Task ProcessarLeituraAsync_MultiplasRegras_DeveAvaliarTodas()
    {
        var leitura = CriarLeitura(temperatura: 42m, umidadeSolo: 15m);
        var regraTemp = CriarRegra("temperatura", Operador.Maior, 40m, TipoAlerta.Temperatura);
        var regraUmid = CriarRegra("umidade_solo", Operador.Menor, 20m, TipoAlerta.Seca);

        _regraRepoMock.Setup(r => r.GetAtivasByTalhaoIdAsync(leitura.TalhaoId))
            .ReturnsAsync(new[] { regraTemp, regraUmid });
        _alertaRepoMock.Setup(r => r.GetNaoResolvidoByRegraETalhaoAsync(It.IsAny<Guid>(), leitura.TalhaoId))
            .ReturnsAsync((Alerta?)null);
        _alertaRepoMock.Setup(r => r.AddAsync(It.IsAny<Alerta>()))
            .ReturnsAsync((Alerta a) => a);
        _eventPublisherMock.Setup(p => p.PublishAlertaCriadoAsync(It.IsAny<Alerta>()))
            .Returns(Task.CompletedTask);
        _historicoRepoMock.Setup(r => r.GetUltimoByTalhaoIdAsync(leitura.TalhaoId))
            .ReturnsAsync((HistoricoStatusTalhao?)null);
        _historicoRepoMock.Setup(r => r.AddAsync(It.IsAny<HistoricoStatusTalhao>()))
            .ReturnsAsync((HistoricoStatusTalhao h) => h);

        await _service.ProcessarLeituraAsync(leitura);

        _alertaRepoMock.Verify(r => r.AddAsync(It.IsAny<Alerta>()), Times.Exactly(2));
    }

    [Fact]
    public async Task ProcessarLeituraAsync_CampoNulo_NaoDeveAvaliarCampo()
    {
        var leitura = CriarLeitura(temperatura: null);
        var regra = CriarRegra("temperatura", Operador.Maior, 40m);

        _regraRepoMock.Setup(r => r.GetAtivasByTalhaoIdAsync(leitura.TalhaoId))
            .ReturnsAsync(new[] { regra });
        _historicoRepoMock.Setup(r => r.GetUltimoByTalhaoIdAsync(leitura.TalhaoId))
            .ReturnsAsync((HistoricoStatusTalhao?)null);
        _historicoRepoMock.Setup(r => r.AddAsync(It.IsAny<HistoricoStatusTalhao>()))
            .ReturnsAsync((HistoricoStatusTalhao h) => h);

        await _service.ProcessarLeituraAsync(leitura);

        _alertaRepoMock.Verify(r => r.AddAsync(It.IsAny<Alerta>()), Times.Never);
    }

    [Fact]
    public async Task ProcessarLeituraAsync_EventPublisherFalha_NaoDeveLancarExcecao()
    {
        var leitura = CriarLeitura(temperatura: 42m);
        var regra = CriarRegra("temperatura", Operador.Maior, 40m);

        _regraRepoMock.Setup(r => r.GetAtivasByTalhaoIdAsync(leitura.TalhaoId))
            .ReturnsAsync(new[] { regra });
        _alertaRepoMock.Setup(r => r.GetNaoResolvidoByRegraETalhaoAsync(regra.Id, leitura.TalhaoId))
            .ReturnsAsync((Alerta?)null);
        _alertaRepoMock.Setup(r => r.AddAsync(It.IsAny<Alerta>()))
            .ReturnsAsync((Alerta a) => a);
        _eventPublisherMock.Setup(p => p.PublishAlertaCriadoAsync(It.IsAny<Alerta>()))
            .ThrowsAsync(new Exception("RabbitMQ down"));
        _historicoRepoMock.Setup(r => r.GetUltimoByTalhaoIdAsync(leitura.TalhaoId))
            .ReturnsAsync((HistoricoStatusTalhao?)null);
        _historicoRepoMock.Setup(r => r.AddAsync(It.IsAny<HistoricoStatusTalhao>()))
            .ReturnsAsync((HistoricoStatusTalhao h) => h);

        // Não deve lançar exceção - evento é secundário
        await _service.ProcessarLeituraAsync(leitura);

        _alertaRepoMock.Verify(r => r.AddAsync(It.IsAny<Alerta>()), Times.Once);
    }

    // Testes de DeterminarStatusTalhao (via ProcessarLeituraAsync que chama AtualizarHistoricoTalhaoAsync)
    [Theory]
    [InlineData(41, null, null, "Crítico - Temperatura Extrema")]
    [InlineData(36, null, null, "Alerta - Temperatura Alta")]
    [InlineData(4, null, null, "Alerta - Risco de Geada")]
    [InlineData(null, 19, null, "Alerta - Solo Muito Seco")]
    [InlineData(null, 29, null, "Atenção - Solo Seco")]
    [InlineData(null, 91, null, "Alerta - Solo Encharcado")]
    [InlineData(null, 81, null, "Atenção - Solo Muito Úmido")]
    [InlineData(null, null, 51, "Atenção - Chuva Intensa")]
    [InlineData(25, 50, 10, "Normal")]
    public async Task ProcessarLeituraAsync_DeterminarStatusTalhao_DeveGerarStatusCorreto(
        int? temp, int? umid, int? precip, string statusEsperado)
    {
        var leitura = CriarLeitura(
            temperatura: temp.HasValue ? (decimal)temp.Value : null,
            umidadeSolo: umid.HasValue ? (decimal)umid.Value : null,
            precipitacao: precip.HasValue ? (decimal)precip.Value : null);

        // Precisa de pelo menos uma regra para não retornar early em ProcessarLeituraAsync
        // Usa regra de campo inexistente para não disparar alerta
        var regraDummy = CriarRegra("radiacao_solar", Operador.Maior, 99999m);
        _regraRepoMock.Setup(r => r.GetAtivasByTalhaoIdAsync(leitura.TalhaoId))
            .ReturnsAsync(new[] { regraDummy });
        _historicoRepoMock.Setup(r => r.GetUltimoByTalhaoIdAsync(leitura.TalhaoId))
            .ReturnsAsync((HistoricoStatusTalhao?)null);
        _historicoRepoMock.Setup(r => r.AddAsync(It.IsAny<HistoricoStatusTalhao>()))
            .ReturnsAsync((HistoricoStatusTalhao h) => h);

        await _service.ProcessarLeituraAsync(leitura);

        _historicoRepoMock.Verify(
            r => r.AddAsync(It.Is<HistoricoStatusTalhao>(h => h.Status == statusEsperado)),
            Times.Once);
    }

    [Fact]
    public async Task ProcessarLeituraAsync_MesmoStatus_NaoDeveCriarNovoHistorico()
    {
        var leitura = CriarLeitura(temperatura: 25m, umidadeSolo: 50m);
        var historicoExistente = new HistoricoStatusTalhao(leitura.TalhaoId, "Normal");

        var regraDummy = CriarRegra("radiacao_solar", Operador.Maior, 99999m);
        _regraRepoMock.Setup(r => r.GetAtivasByTalhaoIdAsync(leitura.TalhaoId))
            .ReturnsAsync(new[] { regraDummy });
        _historicoRepoMock.Setup(r => r.GetUltimoByTalhaoIdAsync(leitura.TalhaoId))
            .ReturnsAsync(historicoExistente);

        await _service.ProcessarLeituraAsync(leitura);

        _historicoRepoMock.Verify(r => r.AddAsync(It.IsAny<HistoricoStatusTalhao>()), Times.Never);
    }
}
