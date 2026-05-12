using System;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Infrastructure.Identity;

public class AppIdentityUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedDate { get; set; }
}
