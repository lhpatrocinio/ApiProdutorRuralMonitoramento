namespace ProdutorRuralMonitoramento.Domain.Entities;

/// <summary>
/// Alerta gerado pelo sistema de monitoramento
/// </summary>
public class Alerta
{
    /// <summary>
    /// Identificador único do alerta
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Id do talhão (referência externa - AgroCadastro)
    /// </summary>
    public Guid TalhaoId { get; set; }
    
    /// <summary>
    /// Id do produtor (referência externa - AgroAuth)
    /// </summary>
    public Guid ProdutorId { get; set; }
    
    /// <summary>
    /// Id da regra que gerou o alerta
    /// </summary>
    public Guid RegraId { get; set; }
    
    /// <summary>
    /// Tipo do alerta (Seca, Temperatura, etc.)
    /// </summary>
    public TipoAlerta TipoAlerta { get; set; }
    
    /// <summary>
    /// Severidade do alerta
    /// </summary>
    public Severidade Severidade { get; set; }
    
    /// <summary>
    /// Título resumido do alerta
    /// </summary>
    public string Titulo { get; set; } = string.Empty;
    
    /// <summary>
    /// Mensagem detalhada do alerta
    /// </summary>
    public string? Mensagem { get; set; }
    
    /// <summary>
    /// Valor detectado que disparou o alerta
    /// </summary>
    public decimal? ValorDetectado { get; set; }
    
    /// <summary>
    /// Valor limite configurado na regra
    /// </summary>
    public decimal? ValorLimite { get; set; }
    
    /// <summary>
    /// Indica se o alerta foi lido
    /// </summary>
    public bool Lido { get; set; }
    
    /// <summary>
    /// Data/hora em que o alerta foi lido
    /// </summary>
    public DateTime? LidoEm { get; set; }
    
    /// <summary>
    /// Indica se o alerta foi resolvido
    /// </summary>
    public bool Resolvido { get; set; }
    
    /// <summary>
    /// Data/hora em que o alerta foi resolvido
    /// </summary>
    public DateTime? ResolvidoEm { get; set; }
    
    /// <summary>
    /// Id do usuário que resolveu o alerta
    /// </summary>
    public Guid? ResolvidoPor { get; set; }
    
    /// <summary>
    /// Observação sobre a resolução
    /// </summary>
    public string? Observacao { get; set; }
    
    /// <summary>
    /// Data de criação do alerta
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Regra que gerou o alerta
    /// </summary>
    public RegraAlerta? Regra { get; set; }
    
    /// <summary>
    /// Marca o alerta como lido
    /// </summary>
    public void MarcarComoLido()
    {
        Lido = true;
        LidoEm = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Marca o alerta como resolvido
    /// </summary>
    public void Resolver(Guid resolvidoPor, string? observacao = null)
    {
        Resolvido = true;
        ResolvidoEm = DateTime.UtcNow;
        ResolvidoPor = resolvidoPor;
        Observacao = observacao;
    }
}
