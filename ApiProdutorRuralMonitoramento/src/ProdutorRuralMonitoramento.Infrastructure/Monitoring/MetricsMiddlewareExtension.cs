using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace ProdutorRuralMonitoramento.Infrastructure.Monitoring
{
    /// <summary>
    /// Extensões para configuração do middleware de métricas
    /// </summary>
    public static class MetricsMiddlewareExtension
    {
        /// <summary>
        /// Adiciona o serviço de coleta de métricas
        /// </summary>
        /// <param name="services">A coleção de serviços</param>
        /// <returns>A coleção de serviços com o coletor de métricas configurado</returns>
        public static IServiceCollection AddMetricsCollector(this IServiceCollection services)
        {
            // Registra o coletor de métricas como singleton
            services.AddSingleton<MetricsCollector>();
            return services;
        }

        /// <summary>
        /// Usa o middleware de métricas
        /// </summary>
        /// <param name="app">A aplicação</param>
        /// <returns>A aplicação com o middleware de métricas configurado</returns>
        public static IApplicationBuilder UseMetricsMiddleware(this IApplicationBuilder app)
        {
            app.UseMiddleware<MetricsMiddleware>();
            return app;
        }
    }
}
