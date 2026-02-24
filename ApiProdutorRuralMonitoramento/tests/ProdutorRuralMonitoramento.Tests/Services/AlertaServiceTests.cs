using FluentAssertions;
using Moq;
using AutoMapper;
using Microsoft.Extensions.Logging;
using ProdutorRuralMonitoramento.Application.DTOs.Request;
using ProdutorRuralMonitoramento.Application.DTOs.Response;
using ProdutorRuralMonitoramento.Application.Services;
using ProdutorRuralMonitoramento.Domain.Entities;
using ProdutorRuralMonitoramento.Domain.Interfaces;

namespace ProdutorRuralMonitoramento.Tests.Services;

public class AlertaServiceTests
{
    private readonly Mock<IAlertaRepository> _repoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<AlertaService>> _loggerMock;
    private readonly AlertaService _service;

    public AlertaServiceTests()
    {
        _repoMock = new Mock<IAlertaRepository>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<AlertaService>>();
        _service = new AlertaService(_repoMock.Object, _mapperMock.Object, _loggerMock.Object);
    }

    private static Alerta CriarAlerta(Guid? id = null, bool lido = false, bool resolvido = false,
        TipoAlerta tipo = TipoAlerta.Temperatura, Severidade severidade = Severidade.Alta)
    {
        var alerta = new Alerta(
            produtorId: Guid.NewGuid(),
            talhaoId: Guid.NewGuid(),
            regraAlertaId: Guid.NewGuid(),
            tipoAlerta: tipo,
            severidade: severidade,
            titulo: "Alerta Teste",
            mensagem: "Mensagem teste",
            valorLeitura: 42m);
        if (lido) alerta.MarcarComoLido();
        if (resolvido) alerta.Resolver(Guid.NewGuid(), "Resolvido");
        return alerta;
    }

    [Fact]
    public async Task GetAllByProdutorAsync_DeveRetornarAlertasMapeados()
    {
        var produtorId = Guid.NewGuid();
        var alertas = new List<Alerta> { CriarAlerta(), CriarAlerta() };
        var responses = alertas.Select(a => new AlertaResponse { Id = a.Id }).ToList();
        _repoMock.Setup(r => r.GetByProdutorIdAsync(produtorId)).ReturnsAsync(alertas);
        _mapperMock.Setup(m => m.Map<IEnumerable<AlertaResponse>>(alertas)).Returns(responses);

        var result = await _service.GetAllByProdutorAsync(produtorId);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByIdAsync_QuandoExiste_DeveRetornarAlerta()
    {
        var alerta = CriarAlerta();
        var response = new AlertaComRegraResponse { Id = alerta.Id };
        _repoMock.Setup(r => r.GetByIdAsync(alerta.Id)).ReturnsAsync(alerta);
        _mapperMock.Setup(m => m.Map<AlertaComRegraResponse>(alerta)).Returns(response);

        var result = await _service.GetByIdAsync(alerta.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(alerta.Id);
    }

    [Fact]
    public async Task GetByIdAsync_QuandoNaoExiste_DeveRetornarNull()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Alerta?)null);

        var result = await _service.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task MarcarComoLidoAsync_QuandoExiste_DeveMarcarERetornar()
    {
        var alerta = CriarAlerta();
        var response = new AlertaResponse { Id = alerta.Id, Lido = true };
        _repoMock.Setup(r => r.GetByIdAsync(alerta.Id)).ReturnsAsync(alerta);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Alerta>())).Returns(Task.CompletedTask);
        _mapperMock.Setup(m => m.Map<AlertaResponse>(It.IsAny<Alerta>())).Returns(response);

        var result = await _service.MarcarComoLidoAsync(alerta.Id);

        result.Lido.Should().BeTrue();
        alerta.Lido.Should().BeTrue();
        alerta.LidoEm.Should().NotBeNull();
    }

    [Fact]
    public async Task MarcarComoLidoAsync_QuandoNaoExiste_DeveLancarKeyNotFoundException()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Alerta?)null);

        var act = async () => await _service.MarcarComoLidoAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task ResolverAsync_QuandoExiste_DeveResolverERetornar()
    {
        var alerta = CriarAlerta();
        var resolvidoPor = Guid.NewGuid();
        var request = new AlertaResolverRequest { Observacao = "Água aplicada" };
        var response = new AlertaResponse { Id = alerta.Id, Resolvido = true };
        _repoMock.Setup(r => r.GetByIdAsync(alerta.Id)).ReturnsAsync(alerta);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Alerta>())).Returns(Task.CompletedTask);
        _mapperMock.Setup(m => m.Map<AlertaResponse>(It.IsAny<Alerta>())).Returns(response);

        var result = await _service.ResolverAsync(alerta.Id, resolvidoPor, request);

        result.Resolvido.Should().BeTrue();
        alerta.Resolvido.Should().BeTrue();
        alerta.ResolvidoPor.Should().Be(resolvidoPor);
        alerta.Observacao.Should().Be("Água aplicada");
    }

    [Fact]
    public async Task ResolverAsync_QuandoNaoExiste_DeveLancarKeyNotFoundException()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Alerta?)null);

        var act = async () => await _service.ResolverAsync(Guid.NewGuid(), Guid.NewGuid(), new AlertaResolverRequest());

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task GetResumoAsync_DeveCalcularResumoCorretamente()
    {
        var produtorId = Guid.NewGuid();
        var alertas = new List<Alerta>
        {
            CriarAlerta(tipo: TipoAlerta.Seca, severidade: Severidade.Critica),
            CriarAlerta(tipo: TipoAlerta.Temperatura, severidade: Severidade.Alta, lido: true),
            CriarAlerta(tipo: TipoAlerta.Precipitacao, severidade: Severidade.Media, resolvido: true),
            CriarAlerta(tipo: TipoAlerta.Geada, severidade: Severidade.Baixa)
        };
        _repoMock.Setup(r => r.GetByProdutorIdAsync(produtorId)).ReturnsAsync(alertas);

        var result = await _service.GetResumoAsync(produtorId);

        result.ProdutorId.Should().Be(produtorId);
        result.TotalAlertas.Should().Be(4);
        result.AlertasNaoLidos.Should().Be(3); // 4 - 1 lido
        result.AlertasNaoResolvidos.Should().Be(3); // 4 - 1 resolvido
        result.AlertasCriticos.Should().Be(1);
        result.AlertasAltos.Should().Be(1);
    }

    [Fact]
    public async Task GetByFiltroAsync_ComTipoAlerta_DeveFiltrar()
    {
        var produtorId = Guid.NewGuid();
        var alertas = new List<Alerta>
        {
            CriarAlerta(tipo: TipoAlerta.Seca),
            CriarAlerta(tipo: TipoAlerta.Temperatura),
            CriarAlerta(tipo: TipoAlerta.Seca)
        };
        var responses = new List<AlertaResponse>
        {
            new() { TipoAlerta = TipoAlerta.Seca },
            new() { TipoAlerta = TipoAlerta.Seca }
        };
        _repoMock.Setup(r => r.GetByProdutorIdAsync(produtorId)).ReturnsAsync(alertas);
        _mapperMock.Setup(m => m.Map<IEnumerable<AlertaResponse>>(It.IsAny<IEnumerable<Alerta>>()))
            .Returns(responses);

        var filtro = new AlertaFiltroRequest { TipoAlerta = TipoAlerta.Seca };
        var result = await _service.GetByFiltroAsync(produtorId, filtro);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task CountNaoLidosAsync_DeveRetornarContagem()
    {
        var produtorId = Guid.NewGuid();
        _repoMock.Setup(r => r.CountNaoLidosAsync(produtorId)).ReturnsAsync(5);

        var result = await _service.CountNaoLidosAsync(produtorId);

        result.Should().Be(5);
    }

    [Fact]
    public async Task CountNaoResolvidosAsync_DeveRetornarContagem()
    {
        var produtorId = Guid.NewGuid();
        _repoMock.Setup(r => r.CountNaoResolvidosAsync(produtorId)).ReturnsAsync(3);

        var result = await _service.CountNaoResolvidosAsync(produtorId);

        result.Should().Be(3);
    }
}
