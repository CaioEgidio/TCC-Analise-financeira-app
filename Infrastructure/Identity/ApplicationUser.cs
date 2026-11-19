// Usa os campos e recursos do Identity para representar a conta, com identificador GUID.
using Microsoft.AspNetCore.Identity;

namespace FinData.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
}