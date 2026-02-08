using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ProdutorRuralMonitoramento.Application.Services.Interfaces;

namespace ProdutorRuralMonitoramento.Infrastructure.Messaging;

/// <summary>
/// Consumer RabbitMQ para processar dados de sensores recebidos da API Sensores
/// </summary>
public class SensorDataConsumer : BackgroundService
{
    private readonly ILogger<SensorDataConsumer> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private const string QueueName = "monitoramento.sensor.data";
    private const string ExchangeName = "agro.sensores.exchange";

    public SensorDataConsumer(
        ILogger<SensorDataConsumer> logger,
        IServiceProvider serviceProvider,
        IConnection connection)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _connection = connection;
        _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SensorDataConsumer iniciando...");

        try
        {
            // Declarar exchange e queue
            await _channel.ExchangeDeclareAsync(
                exchange: ExchangeName,
                type: ExchangeType.Direct,
                durable: true,
                autoDelete: false);

            await _channel.QueueDeclareAsync(
                queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false);

            await _channel.QueueBindAsync(
                queue: QueueName,
                exchange: ExchangeName,
                routingKey: "sensor.leitura.created");

            // Configurar QoS
            await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                _logger.LogInformation("Mensagem recebida: {Message}", message);

                try
                {
                    var sensorData = JsonSerializer.Deserialize<SensorDataMessage>(message, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (sensorData != null)
                    {
                        await ProcessarMensagemAsync(sensorData);
                    }

                    await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar mensagem: {Message}", message);
                    // Rejeitar e não recolocar na fila para evitar loop infinito
                    await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
                }
            };

            await _channel.BasicConsumeAsync(
                queue: QueueName,
                autoAck: false,
                consumer: consumer);

            _logger.LogInformation("SensorDataConsumer aguardando mensagens na fila {Queue}", QueueName);

            // Manter o consumer rodando
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no SensorDataConsumer");
        }
    }

    private async Task ProcessarMensagemAsync(SensorDataMessage message)
    {
        using var scope = _serviceProvider.CreateScope();
        var motorAlertas = scope.ServiceProvider.GetRequiredService<IMotorAlertasService>();

        var sensorEvent = new SensorDataEvent(
            LeituraId: message.LeituraId,
            SensorId: message.SensorId,
            TalhaoId: message.TalhaoId,
            TipoLeitura: message.TipoLeitura,
            Valor: message.Valor,
            DataLeitura: message.DataLeitura
        );

        await motorAlertas.ProcessarLeituraAsync(sensorEvent);
        
        _logger.LogInformation(
            "Leitura processada - Sensor: {SensorId}, Tipo: {Tipo}, Valor: {Valor}",
            message.SensorId, message.TipoLeitura, message.Valor);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("SensorDataConsumer encerrando...");
        await _channel.CloseAsync();
        await base.StopAsync(cancellationToken);
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        base.Dispose();
    }
}

/// <summary>
/// Mensagem recebida da fila de sensores
/// </summary>
public record SensorDataMessage(
    Guid LeituraId,
    Guid SensorId,
    Guid TalhaoId,
    string TipoLeitura,
    decimal Valor,
    DateTime DataLeitura
);
