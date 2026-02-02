using Microsoft.Extensions.DependencyInjection;
using ProdutorRuralMonitoramento.Application.Services;
using ProdutorRuralMonitoramento.Application.Services.Interfaces;

namespace ProdutorRuralMonitoramento.Application
{
    public static class ApplicationBootstrapper
    {
        public static void Register(IServiceCollection services)
        {
            services.AddTransient<IProdutorRuralMonitoramentoService, ProdutorRuralMonitoramentoService>();
        }
    }
}
