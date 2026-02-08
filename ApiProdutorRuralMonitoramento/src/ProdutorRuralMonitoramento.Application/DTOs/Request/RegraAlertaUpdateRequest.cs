using ProdutorRuralMonitoramento.Domain.Entities;

namespace ProdutorRuralMonitoramento.Application.DTOs.Request;

/// <summary>
/// DTO para atualização de regra de alerta
/// </summary>
public record RegraAlertaUpdateRequest
{
    /// <summary>
    /// Nome identificador da regra
    /// </summary>
    public string Nome { get; init; } = string.Empty;
    
    /// <summary>
    /// Descrição detalhada da regra
    /// </summary>
    public string? Descricao { get; init; }
    
    /// <summary>
    /// Campo a ser monitorado
    /// </summary>
    public string Campo { get; init; } = string.Empty;
    
    /// <summary>
    /// Operador de comparação
    /// </summary>
    public Operador Operador { get; init; }
    
    /// <summary>
    /// Valor limite para disparo do alerta
    /// </summary>
    public decimal Valor { get; init; }
    
    /// <summary>
    /// Período para análise em horas
    /// </summary>
    public int? DuracaoHoras { get; init; }
    
    /// <summary>
    /// Tipo do alerta
    /// </summary>
    public TipoAlerta TipoAlerta { get; init; }
    
    /// <summary>
    /// Severidade do alerta
    /// </summary>
    public Severidade Severidade { get; init; }
    
    /// <summary>
    /// Id da cultura específica
    /// </summary>
    public Guid? CulturaId { get; init; }
    
    /// <summary>
    /// Indica se a regra está ativa
    /// </summary>
    public bool Ativo { get; init; }
}
