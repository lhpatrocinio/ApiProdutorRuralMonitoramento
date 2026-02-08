using ProdutorRuralMonitoramento.Domain.Entities;

namespace ProdutorRuralMonitoramento.Application.DTOs.Response;

/// <summary>
/// DTO de resposta para RegraAlerta
/// </summary>
public record RegraAlertaResponse
{
    public Guid Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string? Descricao { get; init; }
    public string Campo { get; init; } = string.Empty;
    public string Operador { get; init; } = string.Empty;
    public decimal Valor { get; init; }
    public int? DuracaoHoras { get; init; }
    public TipoAlerta TipoAlerta { get; init; }
    public string TipoAlertaNome { get; init; } = string.Empty;
    public Severidade Severidade { get; init; }
    public string SeveridadeNome { get; init; } = string.Empty;
    public Guid? CulturaId { get; init; }
    public bool Ativo { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
