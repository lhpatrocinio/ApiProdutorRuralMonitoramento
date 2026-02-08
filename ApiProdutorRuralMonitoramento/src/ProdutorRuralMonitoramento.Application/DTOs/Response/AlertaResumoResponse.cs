namespace ProdutorRuralMonitoramento.Application.DTOs.Response;

/// <summary>
/// DTO com resumo de alertas para dashboard
/// </summary>
public record AlertaResumoResponse
{
    public Guid ProdutorId { get; init; }
    public int TotalAlertas { get; init; }
    public int AlertasNaoLidos { get; init; }
    public int AlertasNaoResolvidos { get; init; }
    public int AlertasCriticos { get; init; }
    public int AlertasAltos { get; init; }
    public int AlertasMedios { get; init; }
    public int AlertasBaixos { get; init; }
    public int AlertasSeca { get; init; }
    public int AlertasTemperatura { get; init; }
    public int AlertasPrecipitacao { get; init; }
    public int AlertasGeada { get; init; }
    public DateTime? UltimoAlerta { get; init; }
}
