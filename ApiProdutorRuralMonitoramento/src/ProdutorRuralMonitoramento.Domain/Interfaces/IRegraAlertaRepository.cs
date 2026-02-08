using ProdutorRuralMonitoramento.Domain.Entities;

namespace ProdutorRuralMonitoramento.Domain.Interfaces;

/// <summary>
/// Interface de repositório para RegrasAlerta
/// </summary>
public interface IRegraAlertaRepository : IRepository<RegraAlerta>
{
    Task<IEnumerable<RegraAlerta>> GetByProdutorIdAsync(Guid produtorId);
    Task<IEnumerable<RegraAlerta>> GetByTalhaoIdAsync(Guid talhaoId);
    Task<IEnumerable<RegraAlerta>> GetAtivasAsync(Guid produtorId);
    Task<IEnumerable<RegraAlerta>> GetAtivasByTalhaoIdAsync(Guid talhaoId);
    Task<IEnumerable<RegraAlerta>> GetByTipoAlertaAsync(TipoAlerta tipo);
    Task<IEnumerable<RegraAlerta>> GetByCulturaIdAsync(Guid? culturaId);
    Task<RegraAlerta?> GetWithAlertasAsync(Guid id);
}
