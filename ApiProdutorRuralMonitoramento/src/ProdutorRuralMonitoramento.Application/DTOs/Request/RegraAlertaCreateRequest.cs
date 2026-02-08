using ProdutorRuralMonitoramento.Domain.Entities;

namespace ProdutorRuralMonitoramento.Application.DTOs.Request;

/// <summary>
/// DTO para criação de regra de alerta
/// </summary>
public record RegraAlertaCreateRequest
{
    /// <summary>
    /// Id do talhão (opcional - NULL aplica a todos os talhões do produtor)
    /// </summary>
    public Guid? TalhaoId { get; init; }
    
    /// <summary>
    /// Nome identificador da regra
    /// </summary>
    public string Nome { get; init; } = string.Empty;
    
    /// <summary>
    /// Descrição detalhada da regra
    /// </summary>
    public string? Descricao { get; init; }
    
    /// <summary>
    /// Campo a ser monitorado (umidade_solo, temperatura, precipitacao, umidade_ar, velocidade_vento)
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
    /// Período para análise em horas (ex: últimas 24h)
    /// </summary>
    public int? DuracaoHoras { get; init; }
    
    /// <summary>
    /// Tipo do alerta (1=Seca, 2=Temperatura, 3=Precipitacao, 4=Geada)
    /// </summary>
    public TipoAlerta TipoAlerta { get; init; }
    
    /// <summary>
    /// Severidade do alerta (1=Baixa, 2=Media, 3=Alta, 4=Critica)
    /// </summary>
    public Severidade Severidade { get; init; }
    
    /// <summary>
    /// Id da cultura específica (NULL = todas as culturas)
    /// </summary>
    public Guid? CulturaId { get; init; }
}
