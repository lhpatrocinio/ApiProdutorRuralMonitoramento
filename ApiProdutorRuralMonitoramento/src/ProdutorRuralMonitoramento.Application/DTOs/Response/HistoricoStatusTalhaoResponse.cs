namespace ProdutorRuralMonitoramento.Application.DTOs.Response;

/// <summary>
/// DTO de resposta para histórico de status de talhão
/// </summary>
public record HistoricoStatusTalhaoResponse
{
    public Guid Id { get; init; }
    public Guid TalhaoId { get; init; }
    public int? StatusAnterior { get; init; }
    public string? StatusAnteriorNome { get; init; }
    public int StatusNovo { get; init; }
    public string StatusNovoNome { get; init; } = string.Empty;
    public Guid? AlertaId { get; init; }
    public string? Motivo { get; init; }
    public DateTime CreatedAt { get; init; }
}
