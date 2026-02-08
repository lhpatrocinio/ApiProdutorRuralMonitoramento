using ProdutorRuralMonitoramento.Domain.Entities;

namespace ProdutorRuralMonitoramento.Application.DTOs.Request;

/// <summary>
/// DTO para filtros de consulta de alertas
/// </summary>
public record AlertaFiltroRequest
{
    /// <summary>
    /// Filtrar por talhão
    /// </summary>
    public Guid? TalhaoId { get; init; }
    
    /// <summary>
    /// Filtrar por tipo de alerta
    /// </summary>
    public TipoAlerta? TipoAlerta { get; init; }
    
    /// <summary>
    /// Filtrar por severidade
    /// </summary>
    public Severidade? Severidade { get; init; }
    
    /// <summary>
    /// Filtrar apenas não lidos
    /// </summary>
    public bool? ApenasNaoLidos { get; init; }
    
    /// <summary>
    /// Filtrar apenas não resolvidos
    /// </summary>
    public bool? ApenasNaoResolvidos { get; init; }
    
    /// <summary>
    /// Data inicial do período
    /// </summary>
    public DateTime? DataInicio { get; init; }
    
    /// <summary>
    /// Data final do período
    /// </summary>
    public DateTime? DataFim { get; init; }
}
