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
/// Evento de dados recebidos de sensor (espelhado do API Sensores)
/// </summary>
public record SensorDataEvent(
    Guid LeituraId,
    Guid SensorId,
    Guid TalhaoId,
    string TipoLeitura,
    decimal Valor,
    DateTime DataLeitura
);
