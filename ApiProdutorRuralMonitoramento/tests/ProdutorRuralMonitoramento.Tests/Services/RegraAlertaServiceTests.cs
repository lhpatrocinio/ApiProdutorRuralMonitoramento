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

public class RegraAlertaServiceTests
{
    private readonly Mock<IRegraAlertaRepository> _repoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<RegraAlertaService>> _loggerMock;
    private readonly RegraAlertaService _service;

    public RegraAlertaServiceTests()
    {
        _repoMock = new Mock<IRegraAlertaRepository>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<RegraAlertaService>>();
        _service = new RegraAlertaService(_repoMock.Object, _mapperMock.Object, _loggerMock.Object);
    }

    private static RegraAlerta CriarRegra(Guid? id = null) => new(
        produtorId: Guid.NewGuid(),
        talhaoId: Guid.NewGuid(),
        nome: "Regra Teste",
        descricao: "Desc",
        campo: CampoMonitorado.Temperatura,
        operador: Operador.Maior,
        valor: 35m,
        tipoAlerta: TipoAlerta.Temperatura,
        severidade: Severidade.Alta);

    [Fact]
    public async Task GetByIdAsync_QuandoExiste_DeveRetornarRegra()
    {
        var regra = CriarRegra();
        var response = new RegraAlertaResponse { Id = regra.Id, Nome = "Regra Teste" };
        _repoMock.Setup(r => r.GetByIdAsync(regra.Id)).ReturnsAsync(regra);
        _mapperMock.Setup(m => m.Map<RegraAlertaResponse>(regra)).Returns(response);

        var result = await _service.GetByIdAsync(regra.Id);

        result.Should().NotBeNull();
        result!.Nome.Should().Be("Regra Teste");
    }

    [Fact]
    public async Task GetByIdAsync_QuandoNaoExiste_DeveRetornarNull()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((RegraAlerta?)null);

        var result = await _service.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_DeveCriarRegraERetornar()
    {
        var produtorId = Guid.NewGuid();
        var request = new RegraAlertaCreateRequest
        {
            TalhaoId = Guid.NewGuid(),
            Nome = "Nova Regra",
            Campo = CampoMonitorado.UmidadeSolo,
            Operador = Operador.Menor,
            Valor = 20m,
            TipoAlerta = TipoAlerta.Seca,
            Severidade = Severidade.Critica
        };
        var response = new RegraAlertaResponse { Nome = "Nova Regra" };
        _repoMock.Setup(r => r.AddAsync(It.IsAny<RegraAlerta>()))
            .ReturnsAsync((RegraAlerta r) => r);
        _mapperMock.Setup(m => m.Map<RegraAlertaResponse>(It.IsAny<RegraAlerta>())).Returns(response);

        var result = await _service.CreateAsync(produtorId, request);

        result.Nome.Should().Be("Nova Regra");
        _repoMock.Verify(r => r.AddAsync(It.Is<RegraAlerta>(ra =>
            ra.ProdutorId == produtorId &&
            ra.Campo == CampoMonitorado.UmidadeSolo &&
            ra.Operador == Operador.Menor)), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_QuandoExiste_DeveAtualizarERetornar()
    {
        var regra = CriarRegra();
        var request = new RegraAlertaUpdateRequest
        {
            Nome = "Atualizada",
            Campo = CampoMonitorado.Precipitacao,
            Operador = Operador.MaiorOuIgual,
            Valor = 50m,
            TipoAlerta = TipoAlerta.Precipitacao,
            Severidade = Severidade.Media
        };
        var response = new RegraAlertaResponse { Nome = "Atualizada" };
        _repoMock.Setup(r => r.GetByIdAsync(regra.Id)).ReturnsAsync(regra);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<RegraAlerta>())).Returns(Task.CompletedTask);
        _mapperMock.Setup(m => m.Map<RegraAlertaResponse>(It.IsAny<RegraAlerta>())).Returns(response);

        var result = await _service.UpdateAsync(regra.Id, request);

        result.Nome.Should().Be("Atualizada");
        regra.Campo.Should().Be(CampoMonitorado.Precipitacao);
    }

    [Fact]
    public async Task UpdateAsync_QuandoNaoExiste_DeveLancarKeyNotFoundException()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((RegraAlerta?)null);

        var act = async () => await _service.UpdateAsync(Guid.NewGuid(), new RegraAlertaUpdateRequest
        {
            Nome = "X", Campo = "temperatura", Operador = Operador.Maior, Valor = 1,
            TipoAlerta = TipoAlerta.Temperatura, Severidade = Severidade.Alta
        });

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_QuandoExiste_DeveDeletar()
    {
        var regra = CriarRegra();
        _repoMock.Setup(r => r.GetByIdAsync(regra.Id)).ReturnsAsync(regra);
        _repoMock.Setup(r => r.DeleteAsync(regra)).Returns(Task.CompletedTask);

        await _service.DeleteAsync(regra.Id);

        _repoMock.Verify(r => r.DeleteAsync(regra), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_QuandoNaoExiste_DeveLancarKeyNotFoundException()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((RegraAlerta?)null);

        var act = async () => await _service.DeleteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task AtivarAsync_DeveAtivarERetornar()
    {
        var regra = CriarRegra();
        regra.Desativar();
        var response = new RegraAlertaResponse { Ativo = true };
        _repoMock.Setup(r => r.GetByIdAsync(regra.Id)).ReturnsAsync(regra);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<RegraAlerta>())).Returns(Task.CompletedTask);
        _mapperMock.Setup(m => m.Map<RegraAlertaResponse>(It.IsAny<RegraAlerta>())).Returns(response);

        var result = await _service.AtivarAsync(regra.Id);

        regra.Ativo.Should().BeTrue();
    }

    [Fact]
    public async Task DesativarAsync_DeveDesativarERetornar()
    {
        var regra = CriarRegra();
        var response = new RegraAlertaResponse { Ativo = false };
        _repoMock.Setup(r => r.GetByIdAsync(regra.Id)).ReturnsAsync(regra);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<RegraAlerta>())).Returns(Task.CompletedTask);
        _mapperMock.Setup(m => m.Map<RegraAlertaResponse>(It.IsAny<RegraAlerta>())).Returns(response);

        var result = await _service.DesativarAsync(regra.Id);

        regra.Ativo.Should().BeFalse();
    }

    [Fact]
    public async Task AtivarAsync_QuandoNaoExiste_DeveLancarKeyNotFoundException()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((RegraAlerta?)null);

        var act = async () => await _service.AtivarAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
