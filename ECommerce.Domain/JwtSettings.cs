using System;

namespace ECommerce.Domain;

public class JwtSettings
{
    public string? SecretKey { get; set; }
    public string? Issuer { get; set; }
    public string? Auidence { get; set; }
    public int ExpirationInMinutes { get; set; }
}
