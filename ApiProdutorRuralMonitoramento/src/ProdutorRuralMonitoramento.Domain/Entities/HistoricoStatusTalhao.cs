namespace ProdutorRuralMonitoramento.Domain.Entities;

/// <summary>
/// Registro histórico de mudanças de status de um talhão
/// </summary>
public class HistoricoStatusTalhao
{
    /// <summary>
    /// Identificador único do registro
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Id do talhão (referência externa - AgroCadastro)
    /// </summary>
    public Guid TalhaoId { get; set; }
    
    /// <summary>
    /// Status do talhão
    /// </summary>
    public string Status { get; set; } = string.Empty;
    
    /// <summary>
    /// Descrição/motivo da mudança
    /// </summary>
    public string? Descricao { get; set; }
    
    /// <summary>
    /// Id da leitura que causou a mudança (se aplicável)
    /// </summary>
    public Guid? LeituraId { get; set; }
    
    /// <summary>
    /// Id do alerta que causou a mudança (se aplicável)
    /// </summary>
    public Guid? AlertaId { get; set; }
    
    /// <summary>
    /// Data/hora da mudança
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Construtor padrão para EF
    /// </summary>
    protected HistoricoStatusTalhao() { }
    
    /// <summary>
    /// Construtor com parâmetros
    /// </summary>
    public HistoricoStatusTalhao(
        Guid talhaoId,
        string status,
        string? descricao = null,
        Guid? leituraId = null,
        Guid? alertaId = null)
    {
        Id = Guid.NewGuid();
        TalhaoId = talhaoId;
        Status = status;
        Descricao = descricao;
        LeituraId = leituraId;
        AlertaId = alertaId;
        CreatedAt = DateTime.UtcNow;
    }
}
