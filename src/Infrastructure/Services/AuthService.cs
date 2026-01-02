using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Abstractions.Interfaces;
using Application.Common.DTOs;
using Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _um;
    private readonly IConfiguration _cfg;
    public AuthService(UserManager<ApplicationUser> um, IConfiguration cfg)
    {
        _um = um;
        _cfg = cfg;
    }

    public async Task<string> RegisterAsync(RegisterDto dto)
    {
        var user = new ApplicationUser { UserName = dto.Email, Email = dto.Email, FirstName = dto.FirstName, LastName = dto.LastName };
        IdentityResult res = await _um.CreateAsync(user, dto.Password);
        if (!res.Succeeded)
        {
            throw new InvalidOperationException(string.Join(";", res.Errors.Select(e => e.Description)));
        }

        // Optionally assign roles/claims
        await _um.AddClaimAsync(user, new Claim("role", "user"));

        return await GenerateTokenAsync(user);
    }

    public async Task<string> LoginAsync(LoginDto dto)
    {
        ApplicationUser? user = await _um.FindByEmailAsync(dto.Email);
        if (user == null)
        {
            throw new InvalidOperationException("Invalid login");
        }

        if (!await _um.CheckPasswordAsync(user, dto.Password))
        {
            throw new InvalidOperationException("Invalid login");
        }

        return await GenerateTokenAsync(user);
    }

    private async Task<string> GenerateTokenAsync(ApplicationUser user)
    {
        string key = _cfg["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key missing");
        string issuer = _cfg["Jwt:Issuer"] ?? "store";
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim("displayName", user.DisplayName ?? string.Empty)
        };

        IList<Claim> userClaims = await _um.GetClaimsAsync(user);
        claims.AddRange(userClaims);

        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        var creds = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(issuer, issuer, claims, expires: DateTime.UtcNow.AddDays(7), signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
