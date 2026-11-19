// Valida as credenciais pelo Identity e usa o esquema Bearer para autenticar.
using FinData.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace FinData.Application.Commands.LoginUser;

public sealed class LoginUserHandler
{
    private readonly SignInManager<ApplicationUser> _signInManager;

    // Recebe os serviços necessários pela injeção de dependência.
    public LoginUserHandler(SignInManager<ApplicationUser> signInManager)
    {
        _signInManager = signInManager;
    }

    // Valida as credenciais pelo Identity e usa o esquema Bearer para autenticar.
    public Task<SignInResult> HandleAsync(LoginUserCommand command)
    {
        _signInManager.AuthenticationScheme = IdentityConstants.BearerScheme;

        // O Identity confere a senha e contabiliza falhas para o bloqueio temporário configurado.
        return _signInManager.PasswordSignInAsync(
            command.Email.Trim(),
            command.Password,
            isPersistent: false,
            lockoutOnFailure: true);
    }
}