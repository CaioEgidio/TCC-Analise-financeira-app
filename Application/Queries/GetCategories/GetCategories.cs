namespace FinData.Application.Queries.GetCategories;

public record CategoryDto(
    Guid Id,
    string Name,
    string? Description);