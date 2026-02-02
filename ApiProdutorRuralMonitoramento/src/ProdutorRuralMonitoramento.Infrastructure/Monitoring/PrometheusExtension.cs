using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;
using Prometheus.DotNetRuntime;
using System;
using System.Diagnostics;

namespace ProdutorRuralMonitoramento.Infrastructure.Monitoring
{
    /// <summary>
    /// Extensões para configuração do Prometheus na aplicação
    /// </summary>
    public static class PrometheusExtension
    {
        /// <summary>
        /// Configura o Prometheus para a aplicação
        /// </summary>
        /// <param name="services">A coleção de serviços</param>
        /// <returns>A coleção de serviços com Prometheus configurado</returns>
        public static IServiceCollection AddPrometheusMonitoring(this IServiceCollection services)
        {
            // Adiciona health checks com métricas do Prometheus
            services.AddHealthChecks()
                .ForwardToPrometheus();

            // Configura coletor de métricas do runtime .NET
            var collector = Prometheus.DotNetRuntime.DotNetRuntimeStatsBuilder
                .Default()
                .StartCollecting();

            // Registra o coletor no container para evitar que seja coletado pelo GC
            services.AddSingleton(collector);

            return services;
        }

        /// <summary>
        /// Configura os middleware do Prometheus na aplicação
        /// </summary>
        /// <param name="app">A aplicação</param>
        /// <returns>A aplicação com os middleware do Prometheus configurados</returns>
        public static IApplicationBuilder UsePrometheusMonitoring(this IApplicationBuilder app)
        {
            // Adiciona middleware para coletar métricas HTTP
            app.UseHttpMetrics(options =>
            {
                // Configura para coletar métricas por endpoint
                options.AddCustomLabel("endpoint", context =>
                {
                    return context.Request.Path.Value ?? "unknown";
                });
            });

            // Expõe o endpoint /metrics do Prometheus
            app.UseMetricServer();

            // Middleware para coletar exceções como métricas
            app.UseExceptionMetrics();

            return app;
        }

        /// <summary>
        /// Configura middleware para contabilizar exceções como métricas
        /// </summary>
        private static IApplicationBuilder UseExceptionMetrics(this IApplicationBuilder app)
        {
            // Contador de exceções
            var exceptionCounter = Metrics
                .CreateCounter(
                    "app_unhandled_exceptions_total",
                    "Counts unhandled exceptions",
                    new CounterConfiguration
                    {
                        LabelNames = new[] { "exception_type" }
                    });

            // Middleware para capturar exceções não tratadas
            app.Use(async (context, next) =>
            {
                try
                {
                    await next();
                }
                catch (Exception ex)
                {
                    // Incrementa o contador com o tipo da exceção
                    exceptionCounter
                        .WithLabels(ex.GetType().Name)
                        .Inc();

                    // Re-lança a exceção para ser tratada pelo middleware de erro
                    throw;
                }
            });

            return app;
        }
    }
}
