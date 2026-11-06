namespace FinData.Application.Queries.GetFinancialSummary;

public record FinancialSummaryDto(
    int Year,
    int Month,
    decimal TotalIncome,
    decimal TotalExpense,
    decimal Balance);