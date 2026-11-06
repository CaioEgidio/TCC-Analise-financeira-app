using FinData.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinData.Infrastructure.Persistence;

public class FinDataDbContext : DbContext
{
    public FinDataDbContext(DbContextOptions<FinDataDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

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
        });

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
        });
    }
}