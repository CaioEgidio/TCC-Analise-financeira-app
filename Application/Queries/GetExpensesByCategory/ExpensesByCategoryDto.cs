namespace FinData.Application.Queries.GetExpensesByCategory;

public record ExpensesByCategoryDto(
    Guid CategoryId,
    string CategoryName,
    decimal Total);