using FinData.Domain.Entities;
using FinData.Domain.Interfaces;

namespace FinData.Application.Commands.CreateCategory;

public class CreateCategoryHandler
{
    private readonly ICategoryRepository _repository;

    public CreateCategoryHandler(ICategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> HandleAsync(
        CreateCategoryCommand command,
        CancellationToken cancellationToken = default)
    {
        var category = new Category(
            command.Name,
            command.Description);

        await _repository.AddAsync(category, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return category.Id;
    }
}

