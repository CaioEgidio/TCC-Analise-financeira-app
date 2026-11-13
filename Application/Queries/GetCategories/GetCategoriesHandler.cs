using FinData.Application.Interfaces;
using FinData.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinData.Application.Queries.GetCategories;

public class GetCategoriesHandler
{
    private readonly FinDataDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetCategoriesHandler(
        FinDataDbContext context,
        ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<CategoryDto>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.Id;

        return await _context.Categories
            .AsNoTracking()
            .Where(category => category.UserId == userId)
            .OrderBy(category => category.Name)
            .Select(category => new CategoryDto(
                category.Id,
                category.Name,
                category.Description))
            .ToListAsync(cancellationToken);
    }
}