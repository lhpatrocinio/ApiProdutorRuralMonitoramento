using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;

namespace ProdutorRuralMonitoramento.Infrastructure.Extensions.Elasticsearch
{
    public static class ElasticsearchExtensions
    {
        public static IServiceCollection AddElasticsearch(this IServiceCollection services, IConfiguration configuration)
        {
            var elasticUri = configuration.GetConnectionString("Elasticsearch") ?? "http://localhost:9200"; 
            return services;
        }
    }
}
