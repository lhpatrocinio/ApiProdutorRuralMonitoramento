using FluentValidation;
using ProdutorRuralMonitoramento.Application.DTOs.Request;
using ProdutorRuralMonitoramento.Domain.Entities;

namespace ProdutorRuralMonitoramento.Application.Validators;

/// <summary>
/// Validador para criação de regra de alerta
/// </summary>
public class RegraAlertaCreateRequestValidator : AbstractValidator<RegraAlertaCreateRequest>
{
    public RegraAlertaCreateRequestValidator()
    {
        // TalhaoId é opcional - se não informado, aplica a todos os talhões do produtor

        RuleFor(x => x.Nome)
            .NotEmpty()
            .WithMessage("O nome da regra é obrigatório")
            .MaximumLength(100)
            .WithMessage("O nome deve ter no máximo 100 caracteres");

        RuleFor(x => x.Descricao)
            .MaximumLength(500)
            .WithMessage("A descrição deve ter no máximo 500 caracteres");

        RuleFor(x => x.Campo)
            .NotEmpty()
            .WithMessage("O campo de monitoramento é obrigatório")
            .Must(BeValidCampo)
            .WithMessage($"O campo deve ser um dos seguintes: {string.Join(", ", CampoMonitorado.TodosCampos)}");

        RuleFor(x => x.Operador)
            .IsInEnum()
            .WithMessage("Operador inválido");

        RuleFor(x => x.Valor)
            .NotNull()
            .WithMessage("O valor de comparação é obrigatório");

        RuleFor(x => x.TipoAlerta)
            .IsInEnum()
            .WithMessage("Tipo de alerta inválido");

        RuleFor(x => x.Severidade)
            .IsInEnum()
            .WithMessage("Severidade inválida");
    }

    private static bool BeValidCampo(string campo)
    {
        if (string.IsNullOrWhiteSpace(campo)) return false;
        return CampoMonitorado.TodosCampos.Contains(campo, StringComparer.OrdinalIgnoreCase);
    }
}
