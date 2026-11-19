// Transporta os dados recebidos para cadastrar uma categoria.
namespace FinData.Application.Commands.CreateCategory;

public record CreateCategoryCommand(
    string Name,
    string? Description);