using System;
using System.ComponentModel.DataAnnotations;

namespace ECommerce.Application.DTOs.Auth;

public class LoginDto
{
    [Required(ErrorMessage = "Kullanıcı adı/Email zorunludur.")]
    public string UserNameOrEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre zorunldur.")]
    public string Password { get; set; } = string.Empty;
}
