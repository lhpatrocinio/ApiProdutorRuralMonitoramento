using ProdutorRuralMonitoramento.Domain.Entities;

namespace ProdutorRuralMonitoramento.Domain.Interfaces;

/// <summary>
/// Interface de repositório para RegrasAlerta
/// </summary>
public interface IRegraAlertaRepository : IRepository<RegraAlerta>
{
    Task<IEnumerable<RegraAlerta>> GetAtivasAsync();
    Task<IEnumerable<RegraAlerta>> GetByTipoAlertaAsync(TipoAlerta tipo);
    Task<IEnumerable<RegraAlerta>> GetByCulturaIdAsync(Guid? culturaId);
    Task<RegraAlerta?> GetWithAlertasAsync(Guid id);
}
