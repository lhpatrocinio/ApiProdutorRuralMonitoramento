using Microsoft.EntityFrameworkCore;
using ProdutorRuralMonitoramento.Domain.Entities;
using ProdutorRuralMonitoramento.Domain.Interfaces;
using ProdutorRuralMonitoramento.Infrastructure.DataBase.EntityFramework.Context;

namespace ProdutorRuralMonitoramento.Infrastructure.DataBase.Repository;

/// <summary>
/// Repositório de Histórico de Status de Talhão
/// </summary>
public class HistoricoStatusTalhaoRepository : IHistoricoStatusTalhaoRepository
{
    private readonly ApplicationDbContext _context;

    public HistoricoStatusTalhaoRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<HistoricoStatusTalhao?> GetByIdAsync(Guid id)
    {
        return await _context.HistoricoStatusTalhao.FindAsync(id);
    }

    public async Task<HistoricoStatusTalhao> AddAsync(HistoricoStatusTalhao historico)
    {
        await _context.HistoricoStatusTalhao.AddAsync(historico);
        await _context.SaveChangesAsync();
        return historico;
    }

    public async Task<IEnumerable<HistoricoStatusTalhao>> GetByTalhaoIdAsync(Guid talhaoId, int limite = 50)
    {
        return await _context.HistoricoStatusTalhao
            .Where(h => h.TalhaoId == talhaoId)
            .OrderByDescending(h => h.CreatedAt)
            .Take(limite)
            .ToListAsync();
    }

    public async Task<HistoricoStatusTalhao?> GetUltimoByTalhaoIdAsync(Guid talhaoId)
    {
        return await _context.HistoricoStatusTalhao
            .Where(h => h.TalhaoId == talhaoId)
            .OrderByDescending(h => h.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<HistoricoStatusTalhao>> GetByPeriodoAsync(Guid talhaoId, DateTime inicio, DateTime fim)
    {
        return await _context.HistoricoStatusTalhao
            .Where(h => h.TalhaoId == talhaoId && h.CreatedAt >= inicio && h.CreatedAt <= fim)
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync();
    }
}
