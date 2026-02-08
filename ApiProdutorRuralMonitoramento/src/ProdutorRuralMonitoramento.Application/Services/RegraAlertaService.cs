using AutoMapper;
using Microsoft.Extensions.Logging;
using ProdutorRuralMonitoramento.Application.DTOs.Request;
using ProdutorRuralMonitoramento.Application.DTOs.Response;
using ProdutorRuralMonitoramento.Application.Services.Interfaces;
using ProdutorRuralMonitoramento.Domain.Entities;
using ProdutorRuralMonitoramento.Domain.Interfaces;

namespace ProdutorRuralMonitoramento.Application.Services;

/// <summary>
/// Implementação do serviço de Regras de Alerta
/// </summary>
public class RegraAlertaService : IRegraAlertaService
{
    private readonly IRegraAlertaRepository _regraAlertaRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<RegraAlertaService> _logger;

    public RegraAlertaService(
        IRegraAlertaRepository regraAlertaRepository,
        IMapper mapper,
        ILogger<RegraAlertaService> logger)
    {
        _regraAlertaRepository = regraAlertaRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<RegraAlertaResponse>> GetAllByProdutorAsync(Guid produtorId)
    {
        var regras = await _regraAlertaRepository.GetByProdutorIdAsync(produtorId);
        return _mapper.Map<IEnumerable<RegraAlertaResponse>>(regras);
    }

    public async Task<RegraAlertaResponse?> GetByIdAsync(Guid id)
    {
        var regra = await _regraAlertaRepository.GetByIdAsync(id);
        if (regra == null) return null;
        return _mapper.Map<RegraAlertaResponse>(regra);
    }

    public async Task<IEnumerable<RegraAlertaResponse>> GetByTalhaoAsync(Guid talhaoId)
    {
        var regras = await _regraAlertaRepository.GetByTalhaoIdAsync(talhaoId);
        return _mapper.Map<IEnumerable<RegraAlertaResponse>>(regras);
    }

    public async Task<IEnumerable<RegraAlertaResponse>> GetAtivasAsync(Guid produtorId)
    {
        var regras = await _regraAlertaRepository.GetAtivasAsync(produtorId);
        return _mapper.Map<IEnumerable<RegraAlertaResponse>>(regras);
    }

    public async Task<RegraAlertaResponse> CreateAsync(Guid produtorId, RegraAlertaCreateRequest request)
    {
        var regra = new RegraAlerta(
            produtorId: produtorId,
            talhaoId: request.TalhaoId,
            nome: request.Nome,
            descricao: request.Descricao,
            campo: request.Campo,
            operador: request.Operador,
            valor: request.Valor,
            tipoAlerta: request.TipoAlerta,
            severidade: request.Severidade
        );

        await _regraAlertaRepository.AddAsync(regra);
        
        _logger.LogInformation("Regra de alerta {RegraId} criada: {RegraName} para produtor {ProdutorId}", 
            regra.Id, regra.Nome, produtorId);
        
        return _mapper.Map<RegraAlertaResponse>(regra);
    }

    public async Task<RegraAlertaResponse> UpdateAsync(Guid id, RegraAlertaUpdateRequest request)
    {
        var regra = await _regraAlertaRepository.GetByIdAsync(id);
        if (regra == null)
            throw new KeyNotFoundException($"Regra de alerta {id} não encontrada");

        regra.Atualizar(
            nome: request.Nome,
            descricao: request.Descricao,
            campo: request.Campo,
            operador: request.Operador,
            valor: request.Valor,
            tipoAlerta: request.TipoAlerta,
            severidade: request.Severidade
        );

        await _regraAlertaRepository.UpdateAsync(regra);
        
        _logger.LogInformation("Regra de alerta {RegraId} atualizada", id);
        
        return _mapper.Map<RegraAlertaResponse>(regra);
    }

    public async Task DeleteAsync(Guid id)
    {
        var regra = await _regraAlertaRepository.GetByIdAsync(id);
        if (regra == null)
            throw new KeyNotFoundException($"Regra de alerta {id} não encontrada");

        await _regraAlertaRepository.DeleteAsync(regra);
        
        _logger.LogInformation("Regra de alerta {RegraId} excluída", id);
    }

    public async Task<RegraAlertaResponse> AtivarAsync(Guid id)
    {
        var regra = await _regraAlertaRepository.GetByIdAsync(id);
        if (regra == null)
            throw new KeyNotFoundException($"Regra de alerta {id} não encontrada");

        regra.Ativar();
        await _regraAlertaRepository.UpdateAsync(regra);
        
        _logger.LogInformation("Regra de alerta {RegraId} ativada", id);
        
        return _mapper.Map<RegraAlertaResponse>(regra);
    }

    public async Task<RegraAlertaResponse> DesativarAsync(Guid id)
    {
        var regra = await _regraAlertaRepository.GetByIdAsync(id);
        if (regra == null)
            throw new KeyNotFoundException($"Regra de alerta {id} não encontrada");

        regra.Desativar();
        await _regraAlertaRepository.UpdateAsync(regra);
        
        _logger.LogInformation("Regra de alerta {RegraId} desativada", id);
        
        return _mapper.Map<RegraAlertaResponse>(regra);
    }
}
