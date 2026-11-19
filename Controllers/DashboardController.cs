// Disponibiliza os totais mensais e as despesas por categoria.
using FinData.Application.Queries.GetExpensesByCategory;
using FinData.Application.Queries.GetFinancialSummary;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinData.Controllers;
// Protege todas as ações deste controller: é necessário enviar um token válido.
[Authorize]
[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly GetFinancialSummaryHandler _handler;

    // Recebe o handler pela injeção de dependência do ASP.NET Core.
    public DashboardController(GetFinancialSummaryHandler handler)
    {
        _handler = handler;
    }

    // Retorna receitas, despesas e saldo do mês; um período inválido retorna 400.
    [HttpGet("summary")]
    public async Task<ActionResult<FinancialSummaryDto>> GetSummary(
        [FromQuery] int year,
        [FromQuery] int month,
        CancellationToken cancellationToken)
    {
        try
        {
            var summary = await _handler.HandleAsync(
                year,
                month,
                cancellationToken);

            return Ok(summary);
        }
        catch (ArgumentException exception)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Filtro inválido",
                detail: exception.Message);
        }
    }

    // Retorna a composição das despesas do mês, agrupada por categoria.
    [HttpGet("expenses-by-category")]
    public async Task<ActionResult<List<ExpensesByCategoryDto>>>
        GetExpensesByCategory(
            [FromServices] GetExpensesByCategoryHandler handler,
            [FromQuery] int year,
            [FromQuery] int month,
            CancellationToken cancellationToken)
    {
        try
        {
            var expenses = await handler.HandleAsync(
                year,
                month,
                cancellationToken);

            return Ok(expenses);
        }
        catch (ArgumentException exception)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Filtro inválido",
                detail: exception.Message);
        }
    }
}