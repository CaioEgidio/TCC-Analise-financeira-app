using FinData.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace FinData.Application.Commands.LoginUser;

public sealed class LoginUserHandler
{
    private readonly SignInManager<ApplicationUser> _signInManager;

    public LoginUserHandler(SignInManager<ApplicationUser> signInManager)
    {
        _signInManager = signInManager;
    }

    public Task<SignInResult> HandleAsync(LoginUserCommand command)
    {
        _signInManager.AuthenticationScheme = IdentityConstants.BearerScheme;

        return _signInManager.PasswordSignInAsync(
            command.Email.Trim(),
            command.Password,
            isPersistent: false,
            lockoutOnFailure: true);
    }
}