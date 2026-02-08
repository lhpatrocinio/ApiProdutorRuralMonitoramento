using ProdutorRuralMonitoramento.Domain.Entities;

namespace ProdutorRuralMonitoramento.Domain.Interfaces;

/// <summary>
/// Interface de repositório para Alertas
/// </summary>
public interface IAlertaRepository
{
    Task<Alerta?> GetByIdAsync(Guid id);
    Task<Alerta> AddAsync(Alerta alerta);
    Task UpdateAsync(Alerta alerta);
    Task<IEnumerable<Alerta>> GetByProdutorIdAsync(Guid produtorId);
    Task<IEnumerable<Alerta>> GetByTalhaoIdAsync(Guid talhaoId);
    Task<IEnumerable<Alerta>> GetNaoLidosAsync(Guid produtorId);
    Task<IEnumerable<Alerta>> GetNaoResolvidosAsync(Guid produtorId);
    Task<IEnumerable<Alerta>> GetBySeveridadeAsync(Severidade severidade);
    Task<IEnumerable<Alerta>> GetByTipoAlertaAsync(TipoAlerta tipo);
    Task<IEnumerable<Alerta>> GetRecentesAsync(Guid produtorId, int quantidade = 10);
    Task<int> CountNaoLidosAsync(Guid produtorId);
    Task<int> CountNaoResolvidosAsync(Guid produtorId);
    Task MarcarComoLidoAsync(Guid alertaId);
    Task MarcarTodosComoLidosAsync(Guid produtorId);
}
