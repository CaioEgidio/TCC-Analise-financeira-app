using FinData.Application.Interfaces;
using FinData.Domain.Entities;
using FinData.Domain.Interfaces;
using FinData.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinData.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly FinDataDbContext _context;
    private readonly ICurrentUser _currentUser;

    public CategoryRepository(
        FinDataDbContext context,
        ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task AddAsync(
        Category category,
        CancellationToken cancellationToken = default)
    {
        await _context.Categories.AddAsync(category, cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.Id;

        return await _context.Categories.AnyAsync(
            category => category.Id == id &&
                        category.UserId == userId,
            cancellationToken);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}