using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProdutorRuralMonitoramento.Application.Services.Interfaces;
using ProdutorRuralMonitoramento.Domain.Interfaces;
using ProdutorRuralMonitoramento.Infrastructure.DataBase.Repository;
using ProdutorRuralMonitoramento.Infrastructure.Messaging;

namespace ProdutorRuralMonitoramento.Infrastructure
{
    public static class InfraBootstrapper
    {
        public static void Register(IServiceCollection services, IConfiguration configuration)
        {
            // Repositórios
            services.AddScoped<IAlertaRepository, AlertaRepository>();
            services.AddScoped<IRegraAlertaRepository, RegraAlertaRepository>();
            services.AddScoped<IHistoricoStatusTalhaoRepository, HistoricoStatusTalhaoRepository>();
            
            // RabbitMQ
            services.AddRabbitMq(configuration);
            services.AddSingleton<IAlertaEventPublisher, AlertaCreatedPublisher>();
            
            // Background Services
            services.AddHostedService<SensorDataConsumer>();
        }
    }
}
