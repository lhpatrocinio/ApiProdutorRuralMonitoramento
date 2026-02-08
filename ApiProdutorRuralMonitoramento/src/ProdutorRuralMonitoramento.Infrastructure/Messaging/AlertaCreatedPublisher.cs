using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using ProdutorRuralMonitoramento.Application.Services.Interfaces;
using ProdutorRuralMonitoramento.Domain.Entities;

namespace ProdutorRuralMonitoramento.Infrastructure.Messaging;

/// <summary>
/// Publisher RabbitMQ para enviar alertas criados para a API Cadastro
/// </summary>
public class AlertaCreatedPublisher : IAlertaEventPublisher
{
    private readonly ILogger<AlertaCreatedPublisher> _logger;
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private const string ExchangeName = "agro.events";

    public AlertaCreatedPublisher(
        ILogger<AlertaCreatedPublisher> logger,
        IConnection connection)
    {
        _logger = logger;
        _connection = connection;
        _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
        
        // Declarar exchange (Topic para compatibilidade com outros serviços)
        _channel.ExchangeDeclareAsync(
            exchange: ExchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false).GetAwaiter().GetResult();
    }

    public async Task PublishAlertaCriadoAsync(Alerta alerta)
    {
        try
        {
            var message = new AlertaCreatedMessage(
                AlertaId: alerta.Id,
                ProdutorId: alerta.ProdutorId,
                TalhaoId: alerta.TalhaoId,
                TipoAlerta: alerta.TipoAlerta.ToString(),
                Severidade: alerta.Severidade.ToString(),
                Titulo: alerta.Titulo,
                Mensagem: alerta.Mensagem,
                ValorLeitura: alerta.ValorLeitura,
                CreatedAt: alerta.CreatedAt
            );

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = new BasicProperties
            {
                Persistent = true,
                ContentType = "application/json",
                Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            };

            // Routing key dinâmica para compatibilidade com pattern alert.created.#
            var routingKey = $"alert.created.{alerta.TalhaoId}";

            await _channel.BasicPublishAsync(
                exchange: ExchangeName,
                routingKey: routingKey,
                mandatory: false,
                basicProperties: properties,
                body: body);

            _logger.LogInformation(
                "Evento AlertCreated publicado - AlertaId: {AlertaId}, Tipo: {Tipo}, Severidade: {Severidade}",
                alerta.Id, alerta.TipoAlerta, alerta.Severidade);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao publicar evento AlertCreated para alerta {AlertaId}", alerta.Id);
            throw;
        }
    }

    public void Dispose()
    {
        _channel?.Dispose();
    }
}

/// <summary>
/// Mensagem de alerta criado para publicação
/// </summary>
public record AlertaCreatedMessage(
    Guid AlertaId,
    Guid ProdutorId,
    Guid TalhaoId,
    string TipoAlerta,
    string Severidade,
    string Titulo,
    string? Mensagem,
    decimal? ValorLeitura,
    DateTime CreatedAt
);
