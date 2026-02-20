using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace ProdutorRuralMonitoramento.Infrastructure.Messaging;

/// <summary>
/// Configuração do RabbitMQ
/// </summary>
public static class RabbitMqSetup
{
    public static IServiceCollection AddRabbitMq(this IServiceCollection services, IConfiguration configuration)
    {
        var rabbitMqSection = configuration.GetSection("RabbitMQ");
        
        services.AddSingleton<IConnection>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<SensorDataConsumer>>();
            var factory = new ConnectionFactory
            {
                HostName = rabbitMqSection["HostName"] ?? "localhost",
                Port = int.Parse(rabbitMqSection["Port"] ?? "5672"),
                UserName = rabbitMqSection["UserName"] ?? "agro",
                Password = rabbitMqSection["Password"] ?? "agro123",
                VirtualHost = rabbitMqSection["VirtualHost"] ?? "/",
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            try
            {
                logger.LogInformation("Conectando ao RabbitMQ em {Host}:{Port}...", factory.HostName, factory.Port);
                var connection = Task.Run(() => factory.CreateConnectionAsync()).GetAwaiter().GetResult();
                logger.LogInformation("Conectado ao RabbitMQ com sucesso!");
                return connection;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Falha ao conectar ao RabbitMQ em {Host}:{Port}", factory.HostName, factory.Port);
                throw;
            }
        });

        return services;
    }
}
