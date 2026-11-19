// DTO: retorna o período, as receitas, as despesas e o saldo mensal.
namespace FinData.Application.Queries.GetFinancialSummary;

public record FinancialSummaryDto(
    int Year,
    int Month,
    decimal TotalIncome,
    decimal TotalExpense,
    decimal Balance);