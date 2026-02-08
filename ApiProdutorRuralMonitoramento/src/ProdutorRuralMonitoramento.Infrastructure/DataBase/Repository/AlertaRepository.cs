using Microsoft.EntityFrameworkCore;
using ProdutorRuralMonitoramento.Domain.Entities;
using ProdutorRuralMonitoramento.Domain.Interfaces;
using ProdutorRuralMonitoramento.Infrastructure.DataBase.EntityFramework.Context;

namespace ProdutorRuralMonitoramento.Infrastructure.DataBase.Repository;

/// <summary>
/// Repositório de Alertas
/// </summary>
public class AlertaRepository : IAlertaRepository
{
    private readonly ApplicationDbContext _context;

    public AlertaRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Alerta?> GetByIdAsync(Guid id)
    {
        return await _context.Alertas
            .Include(a => a.RegraAlerta)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Alerta> AddAsync(Alerta alerta)
    {
        await _context.Alertas.AddAsync(alerta);
        await _context.SaveChangesAsync();
        return alerta;
    }

    public async Task UpdateAsync(Alerta alerta)
    {
        _context.Alertas.Update(alerta);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Alerta>> GetByProdutorIdAsync(Guid produtorId)
    {
        return await _context.Alertas
            .Where(a => a.ProdutorId == produtorId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Alerta>> GetByTalhaoIdAsync(Guid talhaoId)
    {
        return await _context.Alertas
            .Where(a => a.TalhaoId == talhaoId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Alerta>> GetNaoLidosAsync(Guid produtorId)
    {
        return await _context.Alertas
            .Where(a => a.ProdutorId == produtorId && !a.Lido)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Alerta>> GetNaoResolvidosAsync(Guid produtorId)
    {
        return await _context.Alertas
            .Where(a => a.ProdutorId == produtorId && !a.Resolvido)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Alerta>> GetBySeveridadeAsync(Severidade severidade)
    {
        return await _context.Alertas
            .Where(a => a.Severidade == severidade)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Alerta>> GetByTipoAlertaAsync(TipoAlerta tipo)
    {
        return await _context.Alertas
            .Where(a => a.TipoAlerta == tipo)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Alerta>> GetRecentesAsync(Guid produtorId, int quantidade = 10)
    {
        return await _context.Alertas
            .Where(a => a.ProdutorId == produtorId)
            .OrderByDescending(a => a.CreatedAt)
            .Take(quantidade)
            .ToListAsync();
    }

    public async Task<int> CountNaoLidosAsync(Guid produtorId)
    {
        return await _context.Alertas
            .CountAsync(a => a.ProdutorId == produtorId && !a.Lido);
    }

    public async Task<int> CountNaoResolvidosAsync(Guid produtorId)
    {
        return await _context.Alertas
            .CountAsync(a => a.ProdutorId == produtorId && !a.Resolvido);
    }

    public async Task MarcarComoLidoAsync(Guid alertaId)
    {
        var alerta = await _context.Alertas.FindAsync(alertaId);
        if (alerta != null)
        {
            alerta.MarcarComoLido();
            await _context.SaveChangesAsync();
        }
    }

    public async Task MarcarTodosComoLidosAsync(Guid produtorId)
    {
        var alertasNaoLidos = await _context.Alertas
            .Where(a => a.ProdutorId == produtorId && !a.Lido)
            .ToListAsync();

        foreach (var alerta in alertasNaoLidos)
        {
            alerta.MarcarComoLido();
        }

        await _context.SaveChangesAsync();
    }

    public async Task<Alerta?> GetNaoResolvidoByRegraETalhaoAsync(Guid regraId, Guid talhaoId)
    {
        return await _context.Alertas
            .FirstOrDefaultAsync(a => 
                a.RegraAlertaId == regraId && 
                a.TalhaoId == talhaoId && 
                !a.Resolvido);
    }
}
