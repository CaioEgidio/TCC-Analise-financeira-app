using FinData.Domain.Entities;

namespace FinData.Domain.Interfaces;

public interface ICategoryRepository
{
    Task AddAsync(
        Category category,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(
    Guid id,
    CancellationToken cancellationToken = default);
}