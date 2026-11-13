using FinData.Application.Interfaces;
using FinData.Domain.Entities;
using FinData.Domain.Interfaces;

namespace FinData.Application.Commands.CreateCategory;

public class CreateCategoryHandler
{
    private readonly ICategoryRepository _repository;
    private readonly ICurrentUser _currentUser;

    public CreateCategoryHandler(
        ICategoryRepository repository,
        ICurrentUser currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<Guid> HandleAsync(
        CreateCategoryCommand command,
        CancellationToken cancellationToken = default)
    {
        var category = new Category(
            _currentUser.Id,
            command.Name,
            command.Description);

        await _repository.AddAsync(category, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return category.Id;
    }
}
