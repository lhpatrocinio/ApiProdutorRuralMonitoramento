using Microsoft.EntityFrameworkCore;

namespace ProdutorRuralMonitoramento.Infrastructure.DataBase.EntityFramework.Context
{
    public class ApplicationDbContext : DbContext
    {
        /// <summary>  
        /// Initializes a new instance of the <see cref="ApplicationDbContext"/> class with the specified options.  
        /// </summary>  
        /// <param name="options">The options to configure the database context.</param>  
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);          
        }
    }
}
