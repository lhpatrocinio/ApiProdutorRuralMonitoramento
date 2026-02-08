using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using ProdutorRuralMonitoramento.Domain.Entities;
using ProdutorRuralMonitoramento.Domain.Interfaces;
using ProdutorRuralMonitoramento.Infrastructure.DataBase.EntityFramework.Context;

namespace ProdutorRuralMonitoramento.Infrastructure.DataBase.Repository;

/// <summary>
/// Repositório de Regras de Alerta
/// </summary>
public class RegraAlertaRepository : IRegraAlertaRepository
{
    private readonly ApplicationDbContext _context;

    public RegraAlertaRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RegraAlerta?> GetByIdAsync(Guid id)
    {
        return await _context.RegrasAlerta.FindAsync(id);
    }

    public async Task<IEnumerable<RegraAlerta>> GetAllAsync()
    {
        return await _context.RegrasAlerta.ToListAsync();
    }

    public async Task<IEnumerable<RegraAlerta>> FindAsync(Expression<Func<RegraAlerta, bool>> predicate)
    {
        return await _context.RegrasAlerta.Where(predicate).ToListAsync();
    }

    public async Task<RegraAlerta> AddAsync(RegraAlerta entity)
    {
        await _context.RegrasAlerta.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(RegraAlerta entity)
    {
        _context.RegrasAlerta.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(RegraAlerta entity)
    {
        _context.RegrasAlerta.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.RegrasAlerta.AnyAsync(r => r.Id == id);
    }

    public async Task<IEnumerable<RegraAlerta>> GetByProdutorIdAsync(Guid produtorId)
    {
        return await _context.RegrasAlerta
            .Where(r => r.ProdutorId == produtorId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<RegraAlerta>> GetByTalhaoIdAsync(Guid talhaoId)
    {
        return await _context.RegrasAlerta
            .Where(r => r.TalhaoId == talhaoId)
            .ToListAsync();
    }

    public async Task<IEnumerable<RegraAlerta>> GetAtivasAsync(Guid produtorId)
    {
        return await _context.RegrasAlerta
            .Where(r => r.ProdutorId == produtorId && r.Ativo)
            .ToListAsync();
    }

    public async Task<IEnumerable<RegraAlerta>> GetAtivasByTalhaoIdAsync(Guid talhaoId)
    {
        return await _context.RegrasAlerta
            .Where(r => (r.TalhaoId == talhaoId || r.TalhaoId == null) && r.Ativo)
            .ToListAsync();
    }

    public async Task<IEnumerable<RegraAlerta>> GetByTipoAlertaAsync(TipoAlerta tipo)
    {
        return await _context.RegrasAlerta
            .Where(r => r.TipoAlerta == tipo)
            .ToListAsync();
    }

    public async Task<IEnumerable<RegraAlerta>> GetByCulturaIdAsync(Guid? culturaId)
    {
        return await _context.RegrasAlerta
            .Where(r => r.CulturaId == culturaId)
            .ToListAsync();
    }

    public async Task<RegraAlerta?> GetWithAlertasAsync(Guid id)
    {
        return await _context.RegrasAlerta
            .Include(r => r.Alertas)
            .FirstOrDefaultAsync(r => r.Id == id);
    }
}
