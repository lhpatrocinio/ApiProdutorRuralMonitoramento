using ProdutorRuralMonitoramento.Domain.Entities;

namespace ProdutorRuralMonitoramento.Application.DTOs.Response;

/// <summary>
/// DTO de resposta para Alerta
/// </summary>
public record AlertaResponse
{
    public Guid Id { get; init; }
    public Guid TalhaoId { get; init; }
    public Guid ProdutorId { get; init; }
    public Guid RegraId { get; init; }
    public TipoAlerta TipoAlerta { get; init; }
    public string TipoAlertaNome { get; init; } = string.Empty;
    public Severidade Severidade { get; init; }
    public string SeveridadeNome { get; init; } = string.Empty;
    public string Titulo { get; init; } = string.Empty;
    public string? Mensagem { get; init; }
    public decimal? ValorDetectado { get; init; }
    public decimal? ValorLimite { get; init; }
    public bool Lido { get; init; }
    public DateTime? LidoEm { get; init; }
    public bool Resolvido { get; init; }
    public DateTime? ResolvidoEm { get; init; }
    public Guid? ResolvidoPor { get; init; }
    public string? Observacao { get; init; }
    public DateTime CreatedAt { get; init; }
}
