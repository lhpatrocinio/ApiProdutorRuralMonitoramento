using Microsoft.EntityFrameworkCore;
using ProdutorRuralMonitoramento.Domain.Entities;

namespace ProdutorRuralMonitoramento.Infrastructure.DataBase.EntityFramework.Context
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Alerta> Alertas { get; set; }
        public DbSet<RegraAlerta> RegrasAlerta { get; set; }
        public DbSet<HistoricoStatusTalhao> HistoricoStatusTalhao { get; set; }
        
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
            
            // Configuração de Alerta
            builder.Entity<Alerta>(entity =>
            {
                entity.ToTable("Alertas");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Titulo).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Mensagem).HasMaxLength(1000);
                entity.Property(e => e.Observacao).HasMaxLength(500);
                entity.Property(e => e.ValorLeitura).HasColumnType("decimal(18,4)");
                entity.Property(e => e.TipoAlerta).HasConversion<int>();
                entity.Property(e => e.Severidade).HasConversion<int>();
                
                entity.HasOne(e => e.RegraAlerta)
                    .WithMany(r => r.Alertas)
                    .HasForeignKey(e => e.RegraAlertaId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasIndex(e => e.ProdutorId);
                entity.HasIndex(e => e.TalhaoId);
                entity.HasIndex(e => e.RegraAlertaId);
                entity.HasIndex(e => new { e.ProdutorId, e.Lido });
                entity.HasIndex(e => new { e.ProdutorId, e.Resolvido });
            });
            
            // Configuração de RegraAlerta
            builder.Entity<RegraAlerta>(entity =>
            {
                entity.ToTable("RegrasAlerta");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nome).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Descricao).HasMaxLength(500);
                entity.Property(e => e.Campo).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Valor).HasColumnType("decimal(18,4)");
                entity.Property(e => e.Operador).HasConversion<int>();
                entity.Property(e => e.TipoAlerta).HasConversion<int>();
                entity.Property(e => e.Severidade).HasConversion<int>();
                
                entity.HasIndex(e => e.ProdutorId);
                entity.HasIndex(e => e.TalhaoId);
                entity.HasIndex(e => e.Ativo);
                entity.HasIndex(e => new { e.ProdutorId, e.Ativo });
            });
            
            // Configuração de HistoricoStatusTalhao
            builder.Entity<HistoricoStatusTalhao>(entity =>
            {
                entity.ToTable("HistoricoStatusTalhao");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Descricao).HasMaxLength(500);
                
                entity.HasIndex(e => e.TalhaoId);
                entity.HasIndex(e => new { e.TalhaoId, e.CreatedAt });
            });
        }
    }
}
