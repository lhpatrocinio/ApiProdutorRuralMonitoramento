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
    /// Status anterior do talhão
    /// </summary>
    public int? StatusAnterior { get; set; }
    
    /// <summary>
    /// Novo status do talhão
    /// </summary>
    public int StatusNovo { get; set; }
    
    /// <summary>
    /// Id do alerta que causou a mudança (se aplicável)
    /// </summary>
    public Guid? AlertaId { get; set; }
    
    /// <summary>
    /// Motivo da mudança de status
    /// </summary>
    public string? Motivo { get; set; }
    
    /// <summary>
    /// Data/hora da mudança
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
