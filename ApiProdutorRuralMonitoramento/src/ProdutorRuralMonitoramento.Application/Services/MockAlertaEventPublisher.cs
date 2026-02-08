using Microsoft.Extensions.Logging;
using ProdutorRuralMonitoramento.Application.Services.Interfaces;
using ProdutorRuralMonitoramento.Domain.Entities;

namespace ProdutorRuralMonitoramento.Application.Services;

/// <summary>
/// Implementação mock do publicador de eventos - usado quando RabbitMQ não está disponível
/// </summary>
public class MockAlertaEventPublisher : IAlertaEventPublisher
{
    private readonly ILogger<MockAlertaEventPublisher> _logger;

    public MockAlertaEventPublisher(ILogger<MockAlertaEventPublisher> logger)
    {
        _logger = logger;
    }

    public Task PublishAlertaCriadoAsync(Alerta alerta)
    {
        _logger.LogWarning(
            "[MOCK] Evento AlertCreated não publicado (RabbitMQ desabilitado) - AlertaId: {AlertaId}, Tipo: {Tipo}",
            alerta.Id, alerta.TipoAlerta);
        return Task.CompletedTask;
    }
}
