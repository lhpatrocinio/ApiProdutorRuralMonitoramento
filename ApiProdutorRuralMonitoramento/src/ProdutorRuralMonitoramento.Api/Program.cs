using Asp.Versioning.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using ProdutorRuralMonitoramento.Api.Extensions.Auth;
using ProdutorRuralMonitoramento.Api.Extensions.Auth.Middleware;
using ProdutorRuralMonitoramento.Api.Extensions.Logs;
using ProdutorRuralMonitoramento.Api.Extensions.Logs.ELK;
using ProdutorRuralMonitoramento.Api.Extensions.Logs.Extension;
using ProdutorRuralMonitoramento.Api.Extensions.Mappers;
using ProdutorRuralMonitoramento.Api.Extensions.Migration;
using ProdutorRuralMonitoramento.Api.Extensions.Swagger;
using ProdutorRuralMonitoramento.Api.Extensions.Swagger.Middleware;
using ProdutorRuralMonitoramento.Api.Extensions.Tracing;
using ProdutorRuralMonitoramento.Api.Extensions.Versioning;
using ProdutorRuralMonitoramento.Application;
using ProdutorRuralMonitoramento.Infrastructure;
using ProdutorRuralMonitoramento.Infrastructure.DataBase.EntityFramework.Context;
using ProdutorRuralMonitoramento.Infrastructure.Monitoring;

var builder = WebApplication.CreateBuilder(args);

builder.AddSerilogConfiguration();
builder.WebHost.UseUrls("http://*:5003");

builder.Services.AddMvcCore(options => options.AddLogRequestFilter());
builder.Services.AddVersioning();
builder.Services.AddSwaggerDocumentation();
builder.Services.AddAutoMapper(typeof(MapperProfile));
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddAuthorizationExtension(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});


#region [DI]

ApplicationBootstrapper.Register(builder.Services);
InfraBootstrapper.Register(builder.Services, builder.Configuration);

// Prometheus monitoring
builder.Services.AddPrometheusMonitoring();

// ELK Stack integration  
builder.Services.AddELKIntegration(builder.Configuration);

// Distributed Tracing with OpenTelemetry + Jaeger
builder.Services.AddDistributedTracing(builder.Configuration);

#endregion

#region [Consumers]
//builder.Services.AddSingleton<RabbitMqSetup>();
//builder.Services.AddHostedService<UserCreatedConsumer>();

#endregion

var app = builder.Build();

// Developer Exception Page apenas em ambiente de desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Middleware para capturar e logar exceções
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Erro não tratado na requisição {Path}", context.Request.Path);
        throw;
    }
});

app.ExecuteMigrations();
var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

app.UseAuthentication();                        // 1�: popula HttpContext.User
app.UseMiddleware<RoleAuthorizationMiddleware>();
app.UseCorrelationId();
app.UseELKIntegration();

app.UseCors("AllowAll");

// Prometheus middleware
app.UsePrometheusMonitoring();

app.UseVersionedSwagger(apiVersionDescriptionProvider);
app.UseAuthorization();                         // 3�: aplica [Authorize]
//app.UseHttpsRedirection();
app.MapControllers();

// Health Check endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live")
});

app.Run();
