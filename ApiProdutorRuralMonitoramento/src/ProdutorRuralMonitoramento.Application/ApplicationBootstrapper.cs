using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ProdutorRuralMonitoramento.Application.Mappings;
using ProdutorRuralMonitoramento.Application.Services;
using ProdutorRuralMonitoramento.Application.Services.Interfaces;

namespace ProdutorRuralMonitoramento.Application
{
    public static class ApplicationBootstrapper
    {
        public static void Register(IServiceCollection services)
        {
            // AutoMapper
            services.AddAutoMapper(typeof(MapperProfile));

            // FluentValidation
            services.AddValidatorsFromAssemblyContaining<MapperProfile>();

            // Services
            services.AddScoped<IAlertaService, AlertaService>();
            services.AddScoped<IRegraAlertaService, RegraAlertaService>();
            services.AddScoped<IMotorAlertasService, MotorAlertasService>();
            
            // Mock publisher (quando RabbitMQ não está disponível)
            services.AddScoped<IAlertaEventPublisher, MockAlertaEventPublisher>();
        }
    }
}
