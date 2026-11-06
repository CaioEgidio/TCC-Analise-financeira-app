using FinData.Domain.Interfaces;

namespace FinData.Application.Commands.DeleteTransaction;

public class DeleteTransactionHandler
{
    private readonly ITransactionRepository _repository;

    public DeleteTransactionHandler(ITransactionRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> HandleAsync(
        DeleteTransactionCommand command,
        CancellationToken cancellationToken = default)
    {
        var transaction = await _repository.GetByIdAsync(
            command.Id,
            cancellationToken);

        if (transaction is null)
            return false;

        _repository.Remove(transaction);

        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }
}