using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

            return factory.CreateConnectionAsync().GetAwaiter().GetResult();
        });

        return services;
    }
}
