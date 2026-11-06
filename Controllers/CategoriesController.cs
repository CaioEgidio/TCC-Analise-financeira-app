using FinData.Application.Commands.CreateCategory;
using Microsoft.AspNetCore.Mvc;
using FinData.Application.Queries.GetCategories;

namespace FinData.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly CreateCategoryHandler _handler;

    public CategoriesController(CreateCategoryHandler handler)
    {
        _handler = handler;
    }
    
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateCategoryCommand command,
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
    public async Task<ActionResult<List<CategoryDto>>> GetAll(
    [FromServices] GetCategoriesHandler handler,
    CancellationToken cancellationToken)
    {
    var categories = await handler.HandleAsync(cancellationToken);

    return Ok(categories);
    }
}