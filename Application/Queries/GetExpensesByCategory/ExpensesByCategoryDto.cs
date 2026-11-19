// DTO: retorna a identificação da categoria e a soma de suas despesas.
namespace FinData.Application.Queries.GetExpensesByCategory;

public record ExpensesByCategoryDto(
    Guid CategoryId,
    string CategoryName,
    decimal Total);