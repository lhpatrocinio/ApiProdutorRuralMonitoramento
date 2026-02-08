namespace ProdutorRuralMonitoramento.Application.DTOs.Request;

/// <summary>
/// DTO para resolver um alerta
/// </summary>
public record AlertaResolverRequest
{
    /// <summary>
    /// Observação sobre a resolução do alerta
    /// </summary>
    public string? Observacao { get; init; }
}
