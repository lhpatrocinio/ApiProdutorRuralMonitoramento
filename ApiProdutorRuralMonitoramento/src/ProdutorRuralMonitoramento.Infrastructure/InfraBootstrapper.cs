using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProdutorRuralMonitoramento.Application.Repository;
using ProdutorRuralMonitoramento.Infrastructure.DataBase.Repository;

namespace ProdutorRuralMonitoramento.Infrastructure
{
    public static class InfraBootstrapper
    {
        public static void Register(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IProdutorRuralMonitoramentoRepository, ProdutorRuralMonitoramentoRepository>();         
        }
    }
}
