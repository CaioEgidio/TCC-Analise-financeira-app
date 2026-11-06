using FinData.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinData.Application.Queries.GetCategories;

public class GetCategoriesHandler
{
    private readonly FinDataDbContext _context;

    public GetCategoriesHandler(FinDataDbContext context)
    {
        _context = context;
    }

    public async Task<List<CategoryDto>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .AsNoTracking()
            .OrderBy(category => category.Name)
            .Select(category => new CategoryDto(
                category.Id,
                category.Name,
                category.Description))
            .ToListAsync(cancellationToken);
    }
}