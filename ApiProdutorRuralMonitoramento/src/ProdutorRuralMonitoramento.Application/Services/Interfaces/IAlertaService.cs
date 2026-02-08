using ProdutorRuralMonitoramento.Application.DTOs.Request;
using ProdutorRuralMonitoramento.Application.DTOs.Response;
using ProdutorRuralMonitoramento.Domain.Entities;

namespace ProdutorRuralMonitoramento.Application.Services.Interfaces;

/// <summary>
/// Interface do serviço de Alertas
/// </summary>
public interface IAlertaService
{
    Task<IEnumerable<AlertaResponse>> GetAllByProdutorAsync(Guid produtorId);
    Task<AlertaComRegraResponse?> GetByIdAsync(Guid id);
    Task<IEnumerable<AlertaResponse>> GetByTalhaoAsync(Guid talhaoId);
    Task<IEnumerable<AlertaResponse>> GetNaoLidosAsync(Guid produtorId);
    Task<IEnumerable<AlertaResponse>> GetNaoResolvidosAsync(Guid produtorId);
    Task<IEnumerable<AlertaResponse>> GetByFiltroAsync(Guid produtorId, AlertaFiltroRequest filtro);
    Task<IEnumerable<AlertaResponse>> GetRecentesAsync(Guid produtorId, int quantidade = 10);
    Task<AlertaResumoResponse> GetResumoAsync(Guid produtorId);
    Task<AlertaResponse> MarcarComoLidoAsync(Guid alertaId);
    Task MarcarTodosComoLidosAsync(Guid produtorId);
    Task<AlertaResponse> ResolverAsync(Guid alertaId, Guid resolvidoPor, AlertaResolverRequest request);
    Task<int> CountNaoLidosAsync(Guid produtorId);
    Task<int> CountNaoResolvidosAsync(Guid produtorId);
}
