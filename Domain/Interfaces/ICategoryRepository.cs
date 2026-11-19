// Define as operações de persistência de categorias usadas pela aplicação.
using FinData.Domain.Entities;

namespace FinData.Domain.Interfaces;

public interface ICategoryRepository
{
    // Adiciona a categoria ao contexto; a gravação ocorre em SaveChangesAsync.
    Task AddAsync(
        Category category,
        CancellationToken cancellationToken = default);

    // Confirma as alterações pendentes no banco.
    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
    // Verifica a existência da categoria acessível à conta atual.
    Task<bool> ExistsAsync(
    Guid id,
    CancellationToken cancellationToken = default);
}