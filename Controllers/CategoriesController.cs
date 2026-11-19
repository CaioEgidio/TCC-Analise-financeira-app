// Recebe as requisições de cadastro e consulta de categorias.
using FinData.Application.Commands.CreateCategory;
using Microsoft.AspNetCore.Mvc;
using FinData.Application.Queries.GetCategories;
using Microsoft.AspNetCore.Authorization;

namespace FinData.Controllers;

// Protege todas as ações deste controller: é necessário enviar um token válido.
[Authorize]
[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly CreateCategoryHandler _handler;

    // Recebe o handler pela injeção de dependência do ASP.NET Core.
    public CategoriesController(CreateCategoryHandler handler)
    {
        _handler = handler;
    }
    
    // Recebe o comando pelo corpo JSON e retorna o identificador da categoria criada.
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

    // Retorna as categorias acessíveis ao usuário atual.
    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> GetAll(
    [FromServices] GetCategoriesHandler handler,
    CancellationToken cancellationToken)
    {
    var categories = await handler.HandleAsync(cancellationToken);

    return Ok(categories);
    }
}