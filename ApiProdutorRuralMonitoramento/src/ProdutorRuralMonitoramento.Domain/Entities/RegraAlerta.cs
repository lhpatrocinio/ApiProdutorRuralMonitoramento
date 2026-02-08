namespace ProdutorRuralMonitoramento.Domain.Entities;

/// <summary>
/// Regra de alerta para monitoramento de condições agronômicas
/// </summary>
public class RegraAlerta : BaseEntity
{
    /// <summary>
    /// Id do produtor dono da regra
    /// </summary>
    public Guid ProdutorId { get; set; }
    
    /// <summary>
    /// Id do talhão específico (NULL = todas do produtor)
    /// </summary>
    public Guid? TalhaoId { get; set; }
    
    /// <summary>
    /// Nome identificador da regra
    /// </summary>
    public string Nome { get; set; } = string.Empty;
    
    /// <summary>
    /// Descrição detalhada da regra
    /// </summary>
    public string? Descricao { get; set; }
    
    /// <summary>
    /// Campo a ser monitorado (umidade_solo, temperatura, precipitacao)
    /// </summary>
    public string Campo { get; set; } = string.Empty;
    
    /// <summary>
    /// Operador de comparação
    /// </summary>
    public Operador Operador { get; set; }
    
    /// <summary>
    /// Valor limite para disparo do alerta
    /// </summary>
    public decimal Valor { get; set; }
    
    /// <summary>
    /// Período para análise em horas (ex: últimas 24h)
    /// </summary>
    public int? DuracaoHoras { get; set; }
    
    /// <summary>
    /// Tipo do alerta (Seca, Temperatura, Precipitacao, Geada)
    /// </summary>
    public TipoAlerta TipoAlerta { get; set; }
    
    /// <summary>
    /// Severidade do alerta (Baixa, Média, Alta, Crítica)
    /// </summary>
    public Severidade Severidade { get; set; }
    
    /// <summary>
    /// Id da cultura específica (NULL = todas as culturas)
    /// </summary>
    public Guid? CulturaId { get; set; }
    
    /// <summary>
    /// Indica se a regra está ativa
    /// </summary>
    public bool Ativo { get; set; } = true;
    
    /// <summary>
    /// Alertas gerados por esta regra
    /// </summary>
    public ICollection<Alerta> Alertas { get; set; } = new List<Alerta>();
    
    /// <summary>
    /// Construtor padrão para EF
    /// </summary>
    protected RegraAlerta() { }
    
    /// <summary>
    /// Construtor com parâmetros
    /// </summary>
    public RegraAlerta(
        Guid produtorId,
        Guid? talhaoId,
        string nome,
        string? descricao,
        string campo,
        Operador operador,
        decimal valor,
        TipoAlerta tipoAlerta,
        Severidade severidade)
    {
        ProdutorId = produtorId;
        TalhaoId = talhaoId;
        Nome = nome;
        Descricao = descricao;
        Campo = campo;
        Operador = operador;
        Valor = valor;
        TipoAlerta = tipoAlerta;
        Severidade = severidade;
        Ativo = true;
    }
    
    /// <summary>
    /// Atualiza os dados da regra
    /// </summary>
    public void Atualizar(
        string nome,
        string? descricao,
        string campo,
        Operador operador,
        decimal valor,
        TipoAlerta tipoAlerta,
        Severidade severidade)
    {
        Nome = nome;
        Descricao = descricao;
        Campo = campo;
        Operador = operador;
        Valor = valor;
        TipoAlerta = tipoAlerta;
        Severidade = severidade;
    }
    
    /// <summary>
    /// Ativa a regra
    /// </summary>
    public void Ativar() => Ativo = true;
    
    /// <summary>
    /// Desativa a regra
    /// </summary>
    public void Desativar() => Ativo = false;
}
