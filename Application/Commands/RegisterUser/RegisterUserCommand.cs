using System.ComponentModel.DataAnnotations;

namespace FinData.Application.Commands.RegisterUser;

public sealed class RegisterUserCommand
{
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; init; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string Password { get; init; } = string.Empty;
}