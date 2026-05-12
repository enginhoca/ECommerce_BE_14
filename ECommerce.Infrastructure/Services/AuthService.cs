using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using ECommerce.Application.DTOs.Auth;
using ECommerce.Application.DTOs.Common;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;


namespace ECommerce.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<AppIdentityUser> _userManager;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;

    public AuthService(UserManager<AppIdentityUser> userManager, IMapper mapper, IConfiguration configuration)
    {
        _userManager = userManager;
        _mapper = mapper;
        _configuration = configuration;
    }

    public async Task<Result<UserDto>> GetProfileAsync(string userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await _userManager.FindByIdAsync(userId);
        if(user is null)
        {
            return Result<UserDto>.NotFound("Kullanıcı bulunamadı!");
        }
        var roles = await _userManager.GetRolesAsync(user);
        var userDto = new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            UserName = user.UserName!,
            Email = user.Email!,
            Roles = roles
        };
        return Result<UserDto>.Success(userDto);
    }

    public async Task<Result<LoginResponseDto>> LoginAsync(LoginDto dto, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await _userManager.FindByNameAsync(dto.UserNameOrEmail) ?? 
                    await _userManager.FindByEmailAsync(dto.UserNameOrEmail);
        if(user is null || !await _userManager.CheckPasswordAsync(user, dto.Password) )
        {
            return Result<LoginResponseDto>.Unauthorized("Giriş bilgileri hatalı!");
        }
        var roles = await _userManager.GetRolesAsync(user);
        if(await _userManager.IsLockedOutAsync(user))
        {
            return Result<LoginResponseDto>.Unauthorized("Hesabınız geçici olarak kilitlenmiştir. Lütfen admin ile iletişime geçiniz ya da daha sonra deneyiniz.");
        }


        var token = GenerateJwtToken(user, roles);
        var expiration = DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("JwtSettings:ExpirationInMinutes"));
        var userDto = new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            UserName = user.UserName!,
            Email = user.Email!,
            Roles = roles
        };

        var loginResponseDto = new LoginResponseDto
        {
            Token= token,
            Expiration = expiration,
            User= userDto
        };
        return Result<LoginResponseDto>.Success(loginResponseDto);
    }

    public async Task<Result<UserDto>> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        // Email benzersizliği kontrolü
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser is not null)
        {
            return Result<UserDto>.Conflict("Bu email adresi zaten kayıtlı!");
        }

        var appIdentityUser = new AppIdentityUser
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            UserName = dto.UserName,
            CreatedDate = DateTime.UtcNow,
            EmailConfirmed = true
        };
        var result = await _userManager.CreateAsync(appIdentityUser, dto.Password);
        if (!result.Succeeded)
        {
            var error = result.Errors.Select(e => e.Description).FirstOrDefault();
            return Result<UserDto>.ValidationFailure($"Kullanıcı oluşturulamadı, hata: {error}");
        }
        await _userManager.AddToRoleAsync(appIdentityUser, "Customer");

        var roles = await _userManager.GetRolesAsync(appIdentityUser);

        var userDto = new UserDto
        {
            Id = appIdentityUser.Id,
            FirstName = appIdentityUser.FirstName,
            LastName = appIdentityUser.LastName,
            UserName = appIdentityUser.UserName,
            Email = appIdentityUser.Email,
            Roles = roles
        };
        return Result<UserDto>.Success(userDto);
    }

    private string GenerateJwtToken(AppIdentityUser user, IList<string> roles)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"];
        var issuer = jwtSettings["Issuer"];
        var audince = jwtSettings["Audience"];
        var expirationInMinutes = jwtSettings.GetValue<int>("ExpirationInMinutes");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.Name, user.UserName!),
            new("firstName",user.FirstName),
            new("lastName",user.LastName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        foreach(var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audince,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationInMinutes),
            signingCredentials: credentials
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
        

        /*
            Burda üretilen ve döndürülen string tipteki JWT şunları içeriyor olacak:

            - Header {"alg: "HS256", "typ":"JWT}

            - Payload: {
                "nameid": "4234535dgd34",
                "email": "user@gmail.com",
                "role": ["Admin","Customer"],
                "firstName": "Ahmet",
                "lastName": "Işık",
                "jti": "uniqe-id-degeri",
                "exp": 32534532534
            }

            - Signature: HSMACSHA256(base64(header) + base64(payload), secretKey)

        */
    }
}
