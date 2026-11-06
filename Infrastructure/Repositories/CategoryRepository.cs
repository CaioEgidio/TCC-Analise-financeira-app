using FinData.Domain.Entities;
using FinData.Domain.Interfaces;
using FinData.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinData.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly FinDataDbContext _context;

    public CategoryRepository(FinDataDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(
        Category category,
        CancellationToken cancellationToken = default)
    {
        await _context.Categories.AddAsync(category, cancellationToken);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
    public async Task<bool> ExistsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .AnyAsync(category => category.Id == id, cancellationToken);
    }
}