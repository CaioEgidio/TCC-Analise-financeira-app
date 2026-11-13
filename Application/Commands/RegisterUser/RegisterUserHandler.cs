using FinData.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace FinData.Application.Commands.RegisterUser;

public sealed class RegisterUserHandler
{
    private readonly UserManager<ApplicationUser> _userManager;

    public RegisterUserHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<(IdentityResult Result, Guid UserId)> HandleAsync(
        RegisterUserCommand command)
    {
        var email = command.Email.Trim();

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email
        };

        try
        {
            var result = await _userManager.CreateAsync(user, command.Password);
            return (result, user.Id);
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation,
                ConstraintName: "UserNameIndex"
            })
        {
            // Duas requisições podem tentar cadastrar o mesmo e-mail ao mesmo tempo.
            var error = _userManager.ErrorDescriber.DuplicateEmail(email);
            return (IdentityResult.Failed(error), Guid.Empty);
        }
    }
}