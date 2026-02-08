namespace ProdutorRuralMonitoramento.Application.Services.Interfaces;

/// <summary>
/// Interface do Motor de Alertas - Processa leituras de sensores e gera alertas
/// </summary>
public interface IMotorAlertasService
{
    /// <summary>
    /// Processa uma leitura de sensor e verifica se dispara alertas
    /// </summary>
    /// <param name="sensorData">Dados da leitura do sensor</param>
    Task ProcessarLeituraAsync(SensorDataEvent sensorData);
}

/// <summary>
/// Evento de dados recebidos de sensor (alinhado com SensorDataReceivedEvent do API Sensores)
/// </summary>
public record SensorDataEvent
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
