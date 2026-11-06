namespace FinData.Application.Commands.CreateCategory;

public record CreateCategoryCommand(
    string Name,
    string? Description);