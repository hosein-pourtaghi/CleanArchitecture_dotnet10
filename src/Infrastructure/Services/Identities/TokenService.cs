using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Application.Common.Data;
using Application.Common.DTOs.Identities;
using Application.Common.Interfaces;
using Domain.Entities.Identities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SharedKernel;

namespace Infrastructure.Services.Identities;

public class TokenService : ITokenService
{
    private readonly IApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IRolePermissionService _rolePermissionService;

    // Token settings
    private const int AccessTokenMinutes = 10;           // Initial: 10 minutes
    private const int RefreshTokenDays = 7;
    private const int MaxSlidingWindowHours = 8;        // Max: 8 hours

    public TokenService(
        IApplicationDbContext context,
        IConfiguration configuration,
        IRolePermissionService rolePermissionService)
    {
        _context = context;
        _configuration = configuration;
        _rolePermissionService = rolePermissionService;
    }

    public TokenInfo GenerateTokens(Guid userId, int tokenVersion, int refreshTokenVersion, List<string> permissions, List<string> roles)
    {
        var now = DateTime.UtcNow;
        var accessTokenExpires = now.AddMinutes(AccessTokenMinutes);
        var refreshTokenExpires = now.AddDays(RefreshTokenDays);

        var accessToken = GenerateAccessToken(userId, tokenVersion, permissions, roles, now, accessTokenExpires);
        var refreshToken = GenerateRefreshToken(userId, refreshTokenVersion, now, refreshTokenExpires);

        return new TokenInfo(
            AccessToken: accessToken.token,
            RefreshToken: refreshToken.token,
            IssuedAt: now,
            ExpiresAt: accessTokenExpires,
            RefreshTokenExpiresAt: refreshTokenExpires,
            TokenVersion: tokenVersion,
            RefreshTokenVersion: refreshTokenVersion
        );
    }

    private (string token, string tokenId) GenerateAccessToken(
        Guid userId,
        int tokenVersion,
        List<string> permissions,
        List<string> roles,
        DateTime now,
        DateTime expires)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        { 
            // Standard .NET claims
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, string.Empty),
            // JWT-specific claims
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            // Custom claims
            new("token_version", tokenVersion.ToString()),
            new("type", "access")
        };

        #region Add Roles to token
        foreach (var role in roles.Distinct())
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
        #endregion

        #region Add Permissions to token
        //// Add permissions
        //foreach (var permission in permissions.Distinct())
        //{
        //    claims.Add(new Claim("permission", permission));
        //}
        #endregion

        // ✅ CORRECT: Create header separately
        var header = new JwtHeader(creds);
        header["kid"] = _configuration["Jwt:Key"]!;  // Add kid to header
        var token = new JwtSecurityToken(
            header,  // Pass header as first parameter
            new JwtPayload(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                notBefore: now,
                expires: expires,
                issuedAt: now
            )
        );
        //var token = new JwtSecurityToken(
        //    issuer: _configuration["Jwt:Issuer"],
        //    audience: _configuration["Jwt:Audience"],
        //    claims: claims,
        //    notBefore: now,
        //    expires: expires,
        //    signingCredentials: creds       
        //);

        return (new JwtSecurityTokenHandler().WriteToken(token), token.Id);
    }

    private (string token, string tokenId) GenerateRefreshToken(
        Guid userId,
        int refreshTokenVersion,
        DateTime now,
        DateTime expires)
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        var refreshToken = Convert.ToBase64String(randomBytes);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            // Standard .NET claims
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, string.Empty),
            // JWT-specific claims
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            // Custom claims
            new("token_version", refreshTokenVersion.ToString()),
            new("type", "refresh")
        };
        // ✅ CORRECT: Create header separately
        var header = new JwtHeader(creds);
        header["kid"] = _configuration["Jwt:Key"]!;  // Add kid to header
        var token = new JwtSecurityToken(
            header,  // Pass header as first parameter
            new JwtPayload(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                notBefore: now,
                expires: expires,
                issuedAt: now
            )
        );
        //var token = new JwtSecurityToken(
        //    issuer: _configuration["Jwt:Issuer"],
        //    audience: _configuration["Jwt:Audience"],
        //    claims: claims,
        //    notBefore: now,
        //    expires: expires,
        //    signingCredentials: creds
        //);

        return (new JwtSecurityTokenHandler().WriteToken(token), token.Id);
    }

    public (Guid? UserId, string? TokenId, int? TokenVersion) ValidateAccessToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)),
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Audience"],
                ClockSkew = TimeSpan.Zero,
                NameClaimType = ClaimTypes.NameIdentifier,
                RoleClaimType = ClaimTypes.Role
            };

            var principal = handler.ValidateToken(token, validationParameters, out var validatedToken);

            if (validatedToken is not JwtSecurityToken jwtToken)
                return (null, null, null);

            var type = jwtToken.Claims.FirstOrDefault(c => c.Type == "type")?.Value;
            if (type != "access")
                return (null, null, null);

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var tokenId = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
            var tokenVersionClaim = principal.FindFirst("token_version")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || string.IsNullOrEmpty(tokenId))
                return (null, null, null);

            return (Guid.Parse(userIdClaim), tokenId, int.Parse(tokenVersionClaim!));
        }
        catch
        {
            return (null, null, null);
        }
    }

    public (Guid? UserId, string? TokenId, int? RefreshTokenVersion) ValidateRefreshToken(string refreshToken)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)),
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Audience"],
                ClockSkew = TimeSpan.Zero,
                NameClaimType = ClaimTypes.NameIdentifier,
                RoleClaimType = ClaimTypes.Role
            };

            var principal = handler.ValidateToken(refreshToken, validationParameters, out var validatedToken);

            if (validatedToken is not JwtSecurityToken jwtToken)
                return (null, null, null);

            var type = jwtToken.Claims.FirstOrDefault(c => c.Type == "type")?.Value;
            if (type != "refresh")
                return (null, null, null);
             
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var tokenId = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
            var tokenVersionClaim = principal.FindFirst("token_version")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || string.IsNullOrEmpty(tokenId))
                return (null, null, null);

            return (Guid.Parse(userIdClaim), tokenId, int.Parse(tokenVersionClaim!));
        }
        catch
        {
            return (null, null, null);
        }
    }

    public async Task<Result> InvalidateTokenAsync(string tokenId, Guid? userId, string? reason = null)
    {
        try
        {
            // Simply blacklist the token by its ID (JTI)
            // Store for 1 day or until the token naturally expires, whichever is longer
            var blacklistEntry = new TokenBlacklist
            {
                Id = Guid.NewGuid(),
                TokenId = tokenId,
                UserId = userId,
                ExpiresAt = DateTime.UtcNow.AddDays(1), // Keep in blacklist for 1 day
                BlacklistedAt = DateTime.UtcNow,
                Reason = reason
            };

            _context.TokenBlacklist.Add(blacklistEntry);
            await _context.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to invalidate token: {ex.Message}");
        }
    }

    public async Task<bool> IsTokenBlacklistedAsync(string tokenId)
    {
        var isBlacklisted = await _context.TokenBlacklist
            .AnyAsync(t => t.TokenId == tokenId && t.ExpiresAt > DateTime.UtcNow);

        return isBlacklisted;
    }

    public async Task<Result<TokenInfo>> ExtendTokenAsync(string refreshToken)
    {
        var (userId, _, refreshTokenVersion) = ValidateRefreshToken(refreshToken);

        if (userId == null || refreshTokenVersion == null)
            return Result.Failure<TokenInfo>("Invalid refresh token");

        var user = await _context.Users.FindAsync(userId.Value);
        if (user == null)
            return Result.Failure<TokenInfo>("User not found");

        if (user.RefreshTokenVersion != refreshTokenVersion)
            return Result.Failure<TokenInfo>("Refresh token has been revoked");

        // Get user permissions and roles
        var permissionsResult = await _rolePermissionService.GetUserPermissionsAsync(userId.Value);
        var rolesResult = await _rolePermissionService.GetUserRolesAsync(userId.Value);

        if (!permissionsResult.IsSuccess || !rolesResult.IsSuccess)
            return Result.Failure<TokenInfo>("Failed to get user permissions/roles");

        var permissions = permissionsResult.Value.Select(p => p.Name).ToList();
        var roles = rolesResult.Value.Select(r => r.Name).ToList();

        // Generate new tokens with updated version
        var tokens = GenerateTokens(userId.Value, user.TokenVersion, user.RefreshTokenVersion, permissions, roles);

        return Result.Success(tokens);
    }
}
