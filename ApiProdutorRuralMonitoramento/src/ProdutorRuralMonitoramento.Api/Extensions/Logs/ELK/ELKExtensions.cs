using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using System.Diagnostics.CodeAnalysis;

namespace ProdutorRuralMonitoramento.Api.Extensions.Logs.ELK
{
    /// <summary>
    /// Extensões para configuração ELK Stack na aplicação
    /// Implementa centralização de logs via Logstash seguindo padrões FIAP Phase 3
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class ELKExtensions
    {
        /// <summary>
        /// Configura ELK Stack integration para o microserviço API Monitoramento
        /// </summary>
        /// <param name="services">A coleção de serviços</param>
        /// <param name="configuration">Configuração da aplicação</param>
        /// <returns>A coleção de serviços com ELK configurado</returns>
        public static IServiceCollection AddELKIntegration(this IServiceCollection services, IConfiguration configuration)
        {
            // ELK integration é configurado via Serilog TCP sink
            // Pipeline: Serilog → TCP (port 5044) → Logstash → Elasticsearch → Kibana
            
            // Health check básico adicionado - sem dependency especifica do Logstash
            services.AddHealthChecks();

            return services;
        }

        /// <summary>
        /// Configura middleware ELK para enriquecimento de logs
        /// </summary>
        /// <param name="app">A aplicação</param>
        /// <returns>A aplicação com middleware ELK configurado</returns>
        public static IApplicationBuilder UseELKIntegration(this IApplicationBuilder app)
        {
            // Middleware para enriquecer logs com contexto ELK
            // Correlation ID já configurado via CorrelationIdMiddleware existente
            // LogRequestActionFilter já captura HTTP request/response dados
            
            return app;
        }
    }
}