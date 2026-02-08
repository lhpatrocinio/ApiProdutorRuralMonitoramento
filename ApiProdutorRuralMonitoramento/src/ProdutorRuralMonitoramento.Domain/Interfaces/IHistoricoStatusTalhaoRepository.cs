using ProdutorRuralMonitoramento.Domain.Entities;

namespace ProdutorRuralMonitoramento.Domain.Interfaces;

/// <summary>
/// Interface de repositório para HistoricoStatusTalhao
/// </summary>
public interface IHistoricoStatusTalhaoRepository
{
    Task<HistoricoStatusTalhao?> GetByIdAsync(Guid id);
    Task<HistoricoStatusTalhao> AddAsync(HistoricoStatusTalhao historico);
    Task<IEnumerable<HistoricoStatusTalhao>> GetByTalhaoIdAsync(Guid talhaoId, int limite = 50);
    Task<HistoricoStatusTalhao?> GetUltimoByTalhaoIdAsync(Guid talhaoId);
    Task<IEnumerable<HistoricoStatusTalhao>> GetByPeriodoAsync(Guid talhaoId, DateTime inicio, DateTime fim);
}
