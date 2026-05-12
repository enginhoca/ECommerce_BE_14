using System;
using ECommerce.Application.DTOs.Auth;
using ECommerce.Application.DTOs.Common;

namespace ECommerce.Application.Interfaces;

public interface IAuthService
{
    Task<Result<UserDto>> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken);

    Task<Result<LoginResponseDto>> LoginAsync(LoginDto dto, CancellationToken cancellationToken);

    Task<Result<UserDto>> GetProfileAsync(string userId, CancellationToken cancellationToken);
}
