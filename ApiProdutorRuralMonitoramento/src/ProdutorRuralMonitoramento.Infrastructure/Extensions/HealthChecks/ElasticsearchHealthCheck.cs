using Microsoft.Extensions.Diagnostics.HealthChecks;
using Nest;

namespace ProdutorRuralMonitoramento.Infrastructure.Extensions.HealthChecks
{
    public class ElasticsearchHealthCheck : IHealthCheck
    {
        private readonly IElasticClient _client;

        public ElasticsearchHealthCheck(IElasticClient client)
        {
            _client = client;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _client.Cluster.HealthAsync(ct: cancellationToken);
                
                if (response.IsValid)
                {
                    return HealthCheckResult.Healthy("Elasticsearch is healthy", 
                        new Dictionary<string, object>
                        {
                            ["cluster_name"] = response.ClusterName,
                            ["status"] = response.Status.ToString(),
                            ["number_of_nodes"] = response.NumberOfNodes,
                            ["active_primary_shards"] = response.ActivePrimaryShards,
                            ["active_shards"] = response.ActiveShards
                        });
                }

                return HealthCheckResult.Unhealthy("Elasticsearch is not responding correctly", 
                    response.OriginalException);
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Elasticsearch health check failed", ex);
            }
        }
    }
}
