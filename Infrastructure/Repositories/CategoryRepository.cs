// Implementa o acesso às categorias pelo EF Core.
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

    // Recebe o contexto do banco e o serviço que identifica a conta autenticada.
    public CategoryRepository(
        FinDataDbContext context,
        ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    // Adiciona ao contexto, sem confirmar a gravação ainda.
    public async Task AddAsync(
        Category category,
        CancellationToken cancellationToken = default)
    {
        await _context.Categories.AddAsync(category, cancellationToken);
    }

    // Exige correspondência do identificador e do usuário para aceitar a categoria.
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

    // Envia ao banco as alterações acompanhadas pelo contexto.
    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}