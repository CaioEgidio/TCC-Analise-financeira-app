using FinData.Domain.Entities;

namespace FinData.Application.Queries.GetTransactions;

public record TransactionDto(
    Guid Id,
    string Description,
    decimal Amount,
    DateOnly Date,
    TransactionType Type,
    Guid CategoryId,
    string CategoryName);