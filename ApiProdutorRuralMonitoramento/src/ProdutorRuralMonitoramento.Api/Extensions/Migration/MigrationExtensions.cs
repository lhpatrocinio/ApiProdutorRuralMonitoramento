using Microsoft.EntityFrameworkCore;
using ProdutorRuralMonitoramento.Infrastructure.DataBase.EntityFramework.Context;
using System.Diagnostics.CodeAnalysis;

namespace ProdutorRuralMonitoramento.Api.Extensions.Migration
{
    [ExcludeFromCodeCoverage]
    public static class MigrationExtensions
    {
        public static void ExecuteMigrations(this WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();
                
                try
                {
                    // Verifica se há migrações pendentes
                    var pendingMigrations = db.Database.GetPendingMigrations();
                    if (pendingMigrations.Any())
                    {
                        logger.LogInformation("Aplicando {Count} migrações pendentes...", pendingMigrations.Count());
                        db.Database.Migrate();
                    }
                    else
                    {
                        // Apenas garante que o banco existe (não cria tabelas se já existem)
                        logger.LogInformation("Nenhuma migração pendente. Verificando conexão com o banco...");
                        db.Database.CanConnect();
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Erro ao executar migrações. Continuando sem migrações automáticas.");
                }
            }
        }
    }
}
