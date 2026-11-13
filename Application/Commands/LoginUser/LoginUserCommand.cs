using System.ComponentModel.DataAnnotations;

namespace FinData.Application.Commands.LoginUser;

public sealed class LoginUserCommand
{
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; init; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string Password { get; init; } = string.Empty;
}