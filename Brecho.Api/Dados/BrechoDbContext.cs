using Brecho.Api.Modelos;
using Microsoft.EntityFrameworkCore;

namespace Brecho.Api.Dados;

public class BrechoDbContext : DbContext
{
    public BrechoDbContext(DbContextOptions<BrechoDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Peca> Pecas => Set<Peca>();
    public DbSet<Venda> Vendas => Set<Venda>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Usuario>(e =>
        {
            e.HasKey(u => u.Id);
            e.Property(u => u.Nome).IsRequired().HasMaxLength(120);
            e.Property(u => u.Email).IsRequired().HasMaxLength(180);
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.SenhaHash).IsRequired();
            e.Property(u => u.Papel).IsRequired().HasMaxLength(30);
            e.Property(u => u.CriadoEm).IsRequired();
        });

        modelBuilder.Entity<Peca>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Nome).IsRequired().HasMaxLength(150);
            e.Property(p => p.Descricao).IsRequired().HasMaxLength(600);
            e.Property(p => p.Tamanho).IsRequired().HasMaxLength(20);
            e.Property(p => p.Categoria).IsRequired().HasMaxLength(80);
            e.Property(p => p.Condicao).IsRequired().HasMaxLength(80);
            e.Property(p => p.Preco).HasPrecision(10, 2).IsRequired();
            e.Property(p => p.Vendida).IsRequired();
            e.Property(p => p.Ativa).IsRequired();
            e.Property(p => p.CriadaEm).IsRequired();
        });

        modelBuilder.Entity<Venda>(e =>
        {
            e.HasKey(v => v.Id);
            e.Property(v => v.DataVenda).IsRequired();
            e.Property(v => v.FormaPagamento).IsRequired().HasMaxLength(60);
            e.Property(v => v.Observacoes).HasMaxLength(500);
            e.Property(v => v.Situacao).IsRequired().HasMaxLength(40);

            e.HasOne(v => v.Peca)
                .WithMany(p => p.Vendas)
                .HasForeignKey(v => v.PecaId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(v => v.Usuario)
                .WithMany(u => u.Vendas)
                .HasForeignKey(v => v.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}

