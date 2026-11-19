// Obtém o identificador do usuário a partir da identidade autenticada na requisição.
using System.Security.Claims;
using FinData.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace FinData.Infrastructure.Identity;

public sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    // Recebe o serviço que dá acesso ao contexto HTTP da requisição.
    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // Lê a claim de identificação e recusa identidades ausentes ou inválidas.
    public Guid Id
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user?.Identity?.IsAuthenticated != true)
                throw new UnauthorizedAccessException(
                    "É necessário estar autenticado.");

            var value = user.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(value, out var id) || id == Guid.Empty)
                throw new UnauthorizedAccessException(
                    "O identificador do usuário é inválido.");

            return id;
        }
    }
}