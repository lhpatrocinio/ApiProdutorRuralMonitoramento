namespace ProdutorRuralMonitoramento.Application.DTOs.Response;

/// <summary>
/// DTO de resposta para Alerta com detalhes da regra
/// </summary>
public record AlertaComRegraResponse : AlertaResponse
{
    public RegraAlertaResponse? RegraAlerta { get; init; }
}
