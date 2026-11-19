// Coordena a criação de uma categoria para a conta autenticada.
using FinData.Application.Interfaces;
using FinData.Domain.Entities;
using FinData.Domain.Interfaces;

namespace FinData.Application.Commands.CreateCategory;

public class CreateCategoryHandler
{
    private readonly ICategoryRepository _repository;
    private readonly ICurrentUser _currentUser;

    // Recebe os serviços necessários pela injeção de dependência.
    public CreateCategoryHandler(
        ICategoryRepository repository,
        ICurrentUser currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    // Executa a criação de uma categoria para a conta autenticada.
    public async Task<Guid> HandleAsync(
        CreateCategoryCommand command,
        CancellationToken cancellationToken = default)
    {
        // O proprietário vem do token; a entidade valida o nome e a descrição.
        var category = new Category(
            _currentUser.Id,
            command.Name,
            command.Description);

        await _repository.AddAsync(category, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return category.Id;
    }
}
