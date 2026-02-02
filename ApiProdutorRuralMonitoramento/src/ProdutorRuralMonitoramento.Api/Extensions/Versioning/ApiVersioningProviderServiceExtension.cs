using Asp.Versioning;
using System.Diagnostics.CodeAnalysis;

namespace ProdutorRuralMonitoramento.Api.Extensions.Versioning
{
    [ExcludeFromCodeCoverage]
    public static class ApiVersioningProviderServiceExtension
    {
        public static IServiceCollection AddVersioning(this IServiceCollection services)
        {
            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new UrlSegmentApiVersionReader(),
                    new HeaderApiVersionReader("x-api-version"),
                    new MediaTypeApiVersionReader("x-api-version"));
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV"; // Formato: v1, v2, etc.
                options.SubstituteApiVersionInUrl = true;
            });
            return services;
        }
    }
}