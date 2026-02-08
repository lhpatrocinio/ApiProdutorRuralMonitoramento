using ProdutorRuralMonitoramento.Domain.Entities;

namespace ProdutorRuralMonitoramento.Application.Services.Interfaces;

/// <summary>
/// Interface para publicação de eventos de alerta criado via RabbitMQ
/// </summary>
public interface IAlertaEventPublisher
{
    /// <summary>
    /// Publica evento de alerta criado para ser consumido pela API Cadastro
    /// </summary>
    Task PublishAlertaCriadoAsync(Alerta alerta);
}
