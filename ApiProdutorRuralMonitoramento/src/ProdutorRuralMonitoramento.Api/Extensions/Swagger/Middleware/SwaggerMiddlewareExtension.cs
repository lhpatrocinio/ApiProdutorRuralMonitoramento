using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Builder;
using System.Diagnostics.CodeAnalysis;

namespace ProdutorRuralMonitoramento.Api.Extensions.Swagger.Middleware
{
    [ExcludeFromCodeCoverage]
    public static class SwaggerMiddlewareExtension
    {
        public static IApplicationBuilder UseVersionedSwagger(this IApplicationBuilder builder, IApiVersionDescriptionProvider versionProvider)
        {
            builder.UseSwagger(o => o.RouteTemplate = "swagger/{documentName}/swagger.json");

            builder.UseSwaggerUI(options =>
            {

                foreach (var description in versionProvider.ApiVersionDescriptions)
                {
                    options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", $"API {description.GroupName.ToUpperInvariant()}");
                    options.RoutePrefix = "swagger";
                }
            });

            return builder;
        }
    }
}

