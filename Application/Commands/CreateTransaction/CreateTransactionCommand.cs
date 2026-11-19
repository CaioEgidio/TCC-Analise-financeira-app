// Transporta os dados recebidos para criar uma receita ou despesa.
using FinData.Domain.Entities;

namespace FinData.Application.Commands.CreateTransaction;

public record CreateTransactionCommand(
    string Description,
    decimal Amount,
    DateOnly Date,
    TransactionType Type,
    Guid CategoryId);