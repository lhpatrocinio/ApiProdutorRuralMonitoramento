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
    private const string ExchangeName = "agro.events";
    private const string RoutingKey = "sensor.data.#";

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
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false);

            // Declarar queue com os mesmos argumentos usados pela API Sensores (DLX)
            // Se os argumentos não coincidirem, o RabbitMQ rejeita a declaração com PRECONDITION_FAILED
            var queueArgs = new Dictionary<string, object?>
            {
                { "x-dead-letter-exchange", "agro.events.dlx" },
                { "x-dead-letter-routing-key", "dead-letter" },
                { "x-message-ttl", 86400000 } // 24 horas
            };

            await _channel.QueueDeclareAsync(
                queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: queueArgs);

            await _channel.QueueBindAsync(
                queue: QueueName,
                exchange: ExchangeName,
                routingKey: RoutingKey);

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

        var sensorEvent = new SensorDataEvent
        {
            EventId = message.EventId,
            EventDateTime = message.EventDateTime,
            LeituraId = message.LeituraId,
            TalhaoId = message.TalhaoId,
            SensorId = message.SensorId,
            CodigoSensor = message.CodigoSensor,
            UmidadeSolo = message.UmidadeSolo,
            Temperatura = message.Temperatura,
            Precipitacao = message.Precipitacao,
            UmidadeAr = message.UmidadeAr,
            VelocidadeVento = message.VelocidadeVento,
            RadiacaoSolar = message.RadiacaoSolar,
            DataHoraLeitura = message.DataHoraLeitura
        };

        await motorAlertas.ProcessarLeituraAsync(sensorEvent);
        
        _logger.LogInformation(
            "Leitura processada - TalhaoId: {TalhaoId}, Umidade: {Umidade}%, Temp: {Temp}°C",
            message.TalhaoId, message.UmidadeSolo, message.Temperatura);
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
/// Mensagem recebida da fila de sensores (alinhada com SensorDataReceivedEvent do Sensores)
/// </summary>
public record SensorDataMessage
{
    public Guid EventId { get; init; }
    public DateTime EventDateTime { get; init; }
    public Guid LeituraId { get; init; }
    public Guid TalhaoId { get; init; }
    public Guid? SensorId { get; init; }
    public string? CodigoSensor { get; init; }
    public decimal? UmidadeSolo { get; init; }
    public decimal? Temperatura { get; init; }
    public decimal? Precipitacao { get; init; }
    public decimal? UmidadeAr { get; init; }
    public decimal? VelocidadeVento { get; init; }
    public decimal? RadiacaoSolar { get; init; }
    public DateTime DataHoraLeitura { get; init; }
}
