using FluentAssertions;
using FluentValidation.TestHelper;
using ProdutorRuralMonitoramento.Application.DTOs.Request;
using ProdutorRuralMonitoramento.Application.Validators;
using ProdutorRuralMonitoramento.Domain.Entities;

namespace ProdutorRuralMonitoramento.Tests.Validators;

public class RegraAlertaCreateRequestValidatorTests
{
    private readonly RegraAlertaCreateRequestValidator _validator = new();

    [Fact]
    public void Validar_RequestValida_NaoDeveTerErros()
    {
        var request = new RegraAlertaCreateRequest
        {
            Nome = "Regra Teste",
            Campo = CampoMonitorado.Temperatura,
            Operador = Operador.Maior,
            Valor = 35m,
            TipoAlerta = TipoAlerta.Temperatura,
            Severidade = Severidade.Alta
        };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validar_NomeVazio_DeveTerErro()
    {
        var request = new RegraAlertaCreateRequest
        {
            Nome = "",
            Campo = CampoMonitorado.Temperatura,
            Operador = Operador.Maior,
            Valor = 35m,
            TipoAlerta = TipoAlerta.Temperatura,
            Severidade = Severidade.Alta
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Nome);
    }

    [Fact]
    public void Validar_NomeMuitoGrande_DeveTerErro()
    {
        var request = new RegraAlertaCreateRequest
        {
            Nome = new string('A', 101),
            Campo = CampoMonitorado.Temperatura,
            Operador = Operador.Maior,
            Valor = 35m,
            TipoAlerta = TipoAlerta.Temperatura,
            Severidade = Severidade.Alta
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Nome);
    }

    [Fact]
    public void Validar_CampoVazio_DeveTerErro()
    {
        var request = new RegraAlertaCreateRequest
        {
            Nome = "Regra",
            Campo = "",
            Operador = Operador.Maior,
            Valor = 35m,
            TipoAlerta = TipoAlerta.Temperatura,
            Severidade = Severidade.Alta
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Campo);
    }

    [Fact]
    public void Validar_CampoInvalido_DeveTerErro()
    {
        var request = new RegraAlertaCreateRequest
        {
            Nome = "Regra",
            Campo = "campo_invalido",
            Operador = Operador.Maior,
            Valor = 35m,
            TipoAlerta = TipoAlerta.Temperatura,
            Severidade = Severidade.Alta
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Campo);
    }

    [Theory]
    [InlineData("umidade_solo")]
    [InlineData("temperatura")]
    [InlineData("precipitacao")]
    [InlineData("umidade_ar")]
    [InlineData("velocidade_vento")]
    [InlineData("radiacao_solar")]
    public void Validar_CamposValidos_NaoDeveTerErro(string campo)
    {
        var request = new RegraAlertaCreateRequest
        {
            Nome = "Regra",
            Campo = campo,
            Operador = Operador.Maior,
            Valor = 35m,
            TipoAlerta = TipoAlerta.Temperatura,
            Severidade = Severidade.Alta
        };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Campo);
    }

    [Fact]
    public void Validar_DescricaoMuitoGrande_DeveTerErro()
    {
        var request = new RegraAlertaCreateRequest
        {
            Nome = "Regra",
            Descricao = new string('A', 501),
            Campo = CampoMonitorado.Temperatura,
            Operador = Operador.Maior,
            Valor = 35m,
            TipoAlerta = TipoAlerta.Temperatura,
            Severidade = Severidade.Alta
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Descricao);
    }
}
