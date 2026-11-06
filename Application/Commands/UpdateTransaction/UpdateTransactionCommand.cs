using FinData.Domain.Entities;

namespace FinData.Application.Commands.UpdateTransaction;

public record UpdateTransactionCommand(
    string Description,
    decimal Amount,
    DateOnly Date,
    TransactionType Type,
    Guid CategoryId);