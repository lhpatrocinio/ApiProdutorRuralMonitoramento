using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ProdutorRuralMonitoramento.Infrastructure.DataBase.EntityFramework.Context;

/// <summary>
/// Factory para criação do DbContext em tempo de design (migrations)
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=tcp:localhost,1433;Database=AgroMonitoramento;User Id=sa;Password=AgroSolutions@2026!;TrustServerCertificate=true;MultipleActiveResultSets=true;Encrypt=false");

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
