using Asp.Versioning.ApiExplorer;
using Microsoft.OpenApi.Models;
using ProdutorRuralMonitoramento.Api.Extensions.Swagger.Filters;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace ProdutorRuralMonitoramento.Api.Extensions.Swagger
{
    [ExcludeFromCodeCoverage]
    public static class SwaggerServicesExtension
    {
        /// <summary>
        /// Add swagger documentation service on dependency injection container 
        /// </summary>
        public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {

                var _provider = services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();

                foreach (var description in _provider.ApiVersionDescriptions)
                {
                    options.SwaggerDoc(description.GroupName, new OpenApiInfo()
                    {
                        Title = $"ProdutorRuralMonitoramento API - {description.GroupName.ToUpper()}",
                        Version = description.ApiVersion.ToString(),
                        Description = "Documentação gerada automaticamente com Swagger e ApiVersioning."
                    });
                }
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    In = ParameterLocation.Header,
                    Description = @"Insira o token JWT
                      \r\n\r\nExample: 'Bearer 12345abcdef'"
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement()
                  {
                    {
                      new OpenApiSecurityScheme
                      {
                        Reference = new OpenApiReference
                          {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                          },
                          Scheme = "oauth2",
                          Name = "Bearer",
                          In = ParameterLocation.Header,

                        },
                        new List<string>()
                      }
                    });

                options.SchemaFilter<CustomSchemaExcludeFilter>();
                options.DocumentFilter<LowerCaseDocumentFilter>();

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);
            });

            return services;
        }
    }
}
