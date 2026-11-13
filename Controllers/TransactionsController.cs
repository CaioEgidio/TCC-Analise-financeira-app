using FinData.Application.Commands.DeleteTransaction;
using FinData.Application.Commands.CreateTransaction;
using FinData.Application.Commands.UpdateTransaction;
using FinData.Application.Queries.GetTransactions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinData.Controllers;

[Authorize]
[ApiController]
[Route("api/transactions")]
public class TransactionsController : ControllerBase
{
    private readonly CreateTransactionHandler _handler;

    public TransactionsController(CreateTransactionHandler handler)
    {
        _handler = handler;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateTransactionCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var id = await _handler.HandleAsync(
                command,
                cancellationToken);

            return StatusCode(
                StatusCodes.Status201Created,
                new { id });
        }
        catch (ArgumentException exception)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Dados inválidos",
                detail: exception.Message);
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<TransactionDto>>> GetAll(
        [FromServices] GetTransactionsHandler handler,
        [FromQuery] int year,
        [FromQuery] int month,
        CancellationToken cancellationToken)
    {
        try
        {
            var transactions = await handler.HandleAsync(
                year,
                month,
                cancellationToken);

            return Ok(transactions);
        }
        catch (ArgumentException exception)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Filtro inválido",
                detail: exception.Message);
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateTransactionCommand command,
        [FromServices] UpdateTransactionHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated = await handler.HandleAsync(
                id,
                command,
                cancellationToken);

            if (!updated)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Transação não encontrada",
                    detail: "Não existe uma transação com esse identificador.");
            }

            return NoContent();
        }
        catch (ArgumentException exception)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Dados inválidos",
                detail: exception.Message);
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
    [FromRoute] Guid id,
    [FromServices] DeleteTransactionHandler handler,
    CancellationToken cancellationToken)
    {
        var deleted = await handler.HandleAsync(
        new DeleteTransactionCommand(id),
        cancellationToken);

        if (!deleted)
        {
            return Problem(
            statusCode: StatusCodes.Status404NotFound,
            title: "Transação não encontrada",
            detail: "Não existe uma transação com esse identificador.");
        }

    return NoContent();
    }
}