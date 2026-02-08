using ProdutorRuralMonitoramento.Application.DTOs.Request;
using ProdutorRuralMonitoramento.Application.DTOs.Response;

namespace ProdutorRuralMonitoramento.Application.Services.Interfaces;

/// <summary>
/// Interface do serviço de Regras de Alerta
/// </summary>
public interface IRegraAlertaService
{
    Task<IEnumerable<RegraAlertaResponse>> GetAllByProdutorAsync(Guid produtorId);
    Task<RegraAlertaResponse?> GetByIdAsync(Guid id);
    Task<IEnumerable<RegraAlertaResponse>> GetByTalhaoAsync(Guid talhaoId);
    Task<IEnumerable<RegraAlertaResponse>> GetAtivasAsync(Guid produtorId);
    Task<RegraAlertaResponse> CreateAsync(Guid produtorId, RegraAlertaCreateRequest request);
    Task<RegraAlertaResponse> UpdateAsync(Guid id, RegraAlertaUpdateRequest request);
    Task DeleteAsync(Guid id);
    Task<RegraAlertaResponse> AtivarAsync(Guid id);
    Task<RegraAlertaResponse> DesativarAsync(Guid id);
}
