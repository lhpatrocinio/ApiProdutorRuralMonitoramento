using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using System.Diagnostics;

namespace ProdutorRuralMonitoramento.Api.Extensions.Tracing
{
    public static class DistributedTracingExtensions
    {
        private static readonly ActivitySource ActivitySource = new("ApiProdutorRuralMonitoramento");

        /// <summary>
        /// Configura OpenTelemetry Distributed Tracing para Games API
        /// </summary>
        /// <param name="services">Coleção de serviços</param>
        /// <param name="configuration">Configuração da aplicação</param>
        /// <returns>Coleção de serviços configurada</returns>
        public static IServiceCollection AddDistributedTracing(this IServiceCollection services, IConfiguration configuration)
        {
            var serviceName = "fiap-games-api";
            var serviceVersion = "1.0.0";

            try
            {
                services.AddOpenTelemetry()
                    .WithTracing(builder =>
                    {
                        builder
                            .SetResourceBuilder(ResourceBuilder.CreateDefault()
                                .AddService(serviceName, serviceVersion)
                                .AddAttributes(new Dictionary<string, object>
                                {
                                    ["service.namespace"] = "fiap.postech",
                                    ["service.instance.id"] = Environment.MachineName,
                                    ["deployment.environment"] = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development"
                                }))
                            .AddSource("FiapPosTech.*")
                            .AddSource("Games.*")
                            .AddAspNetCoreInstrumentation(options =>
                            {
                                // Filtrar health checks e métricas
                                options.Filter = (httpContext) =>
                                {
                                    var path = httpContext.Request.Path.Value?.ToLower();
                                    return !path?.Contains("/health") == true &&
                                           !path?.Contains("/metrics") == true &&
                                           !path?.Contains("/ready") == true &&
                                           !path?.Contains("/liveness") == true;
                                };
                            })
                            .AddHttpClientInstrumentation()
                            .AddOtlpExporter(options =>
                            {
                                // Detectar ambiente: K8s (tem KUBERNETES_SERVICE_HOST) ou Docker
                                var isKubernetes = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("KUBERNETES_SERVICE_HOST"));
                                var otlpEndpoint = isKubernetes
                                    ? configuration["OpenTelemetry:Otlp:EndpointK8s"] ?? "http://jaeger-service.monitoring.svc.cluster.local:4317"
                                    : configuration["OpenTelemetry:Otlp:Endpoint"] ?? "http://jaeger:4317";

                                options.Endpoint = new Uri(otlpEndpoint);
                                Console.WriteLine($"✅ OpenTelemetry endpoint: {otlpEndpoint} (Environment: {(isKubernetes ? "Kubernetes" : "Docker")}");
                            });
                    });

                // Registrar ActivitySource para injeção de dependência
                services.AddSingleton(ActivitySource);

                Console.WriteLine("✅ OpenTelemetry configurado com sucesso para Games API");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Erro na configuração OpenTelemetry: {ex.Message}");
                // Continue sem tracing se houver erro
            }

            return services;
        }

        /// <summary>
        /// Cria uma nova atividade para operações de negócio
        /// </summary>
        /// <param name="operationName">Nome da operação</param>
        /// <param name="kind">Tipo da atividade</param>
        /// <returns>Nova atividade iniciada</returns>
        public static Activity? StartActivity(string operationName, ActivityKind kind = ActivityKind.Internal)
        {
            return ActivitySource.StartActivity(operationName, kind);
        }

        /// <summary>
        /// Adiciona tags padronizadas a uma atividade
        /// </summary>
        /// <param name="activity">Atividade a ser enriquecida</param>
        /// <param name="tags">Tags a serem adicionadas</param>
        public static void EnrichActivity(this Activity? activity, Dictionary<string, object> tags)
        {
            if (activity == null) return;

            foreach (var tag in tags)
            {
                activity.SetTag(tag.Key, tag.Value);
            }
        }

        /// <summary>
        /// Marca uma atividade como erro
        /// </summary>
        /// <param name="activity">Atividade</param>
        /// <param name="exception">Exceção ocorrida</param>
        public static void SetError(this Activity? activity, Exception exception)
        {
            if (activity == null) return;

            activity.SetStatus(ActivityStatusCode.Error, exception.Message);
            activity.SetTag("error", true);
            activity.SetTag("error.type", exception.GetType().Name);
            activity.SetTag("error.message", exception.Message);
            activity.SetTag("error.stack", exception.StackTrace);
        }

        /// <summary>
        /// Marca uma atividade como bem-sucedida
        /// </summary>
        /// <param name="activity">Atividade</param>
        public static void SetSuccess(this Activity? activity)
        {
            activity?.SetStatus(ActivityStatusCode.Ok);
        }
    }
}