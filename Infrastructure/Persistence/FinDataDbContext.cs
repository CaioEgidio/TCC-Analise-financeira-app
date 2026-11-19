// Conecta o EF Core ao banco e mapeia as tabelas financeiras e do Identity.
using FinData.Domain.Entities;
using FinData.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FinData.Infrastructure.Persistence;

public class FinDataDbContext
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    // Recebe as opções de conexão registradas no Program.cs.
    public FinDataDbContext(
        DbContextOptions<FinDataDbContext> options)
        : base(options)
    {
    }

    // Disponibiliza as tabelas financeiras para consultas e persistência.
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    // Define chaves, limites de campos, tipos de coluna e relacionamentos.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Preserva o mapeamento das tabelas de usuários e demais estruturas do Identity.
        base.OnModelCreating(modelBuilder);

        // Configura a categoria e impede exclusão em cascata do vínculo com o usuário.
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(category => category.Id);

            entity.Property(category => category.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(category => category.Description)
                .HasMaxLength(500);

            entity.Property(category => category.CreatedAt)
                .HasColumnType("timestamp with time zone");

            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(category => category.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configura o lançamento: valor com duas casas, data sem horário e vínculos restritos.
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(transaction => transaction.Id);

            entity.Property(transaction => transaction.Description)
                .IsRequired()
                .HasMaxLength(250);

            entity.Property(transaction => transaction.Amount)
                .HasPrecision(18, 2);

            entity.Property(transaction => transaction.Date)
                .HasColumnType("date");

            entity.Property(transaction => transaction.CreatedAt)
                .HasColumnType("timestamp with time zone");

            entity.HasOne(transaction => transaction.Category)
                .WithMany()
                .HasForeignKey(transaction => transaction.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(transaction => transaction.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}