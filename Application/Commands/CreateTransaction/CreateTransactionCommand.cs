using FinData.Domain.Entities;

namespace FinData.Application.Commands.CreateTransaction;

public record CreateTransactionCommand(
    string Description,
    decimal Amount,
    DateOnly Date,
    TransactionType Type,
    Guid CategoryId);