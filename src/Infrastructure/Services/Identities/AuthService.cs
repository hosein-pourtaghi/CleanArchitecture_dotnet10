using Application.Common.Data;
using Application.Common.DTOs.Identities;
using Application.Common.Interfaces;
using Domain.Entities.Identities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Infrastructure.Services.Identities;




public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly ISessionService _sessionService;
    private readonly IRolePermissionService _rolePermissionService;
    private readonly IApplicationDbContext _context;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService,
        ISessionService sessionService,
        IRolePermissionService rolePermissionService,
        IApplicationDbContext context
        )
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _sessionService = sessionService;
        _rolePermissionService = rolePermissionService;
        _context = context;
    }


    public async Task<Result<LoginResponse>> RegisterAsync(RegisterRequest request, string ipAddress, string? userAgent)
    {
        // Validate password match
        if (request.Password != request.ConfirmPassword)
            return Result.Failure<LoginResponse>("Passwords do not match");

        // Check if email exists
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
            return Result.Failure<LoginResponse>("Email is already registered");

        // Create new user
        var user = new ApplicationUser
        {
            Email = request.Email,
            UserName = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            IsActive = true,
            TokenVersion = 1,
            RefreshTokenVersion = 1,
            AllowMultipleSessions = true,
            MaxConcurrentSessions = 5
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result.Failure<LoginResponse>($"Failed to create user: {errors}");
        }

        // Assign default role (User)
        var defaultRole = await _roleManager.FindByNameAsync("User");
        if (defaultRole != null)
        {
            await _userManager.AddToRoleAsync(user, "User");
        }

        // Get user roles and permissions
        var rolesResult = await _rolePermissionService.GetUserRolesAsync(user.Id);
        var permissionsResult = await _rolePermissionService.GetUserPermissionsAsync(user.Id);

        var roles = rolesResult.IsSuccess ? rolesResult.Value.Select(r => r.Name).ToList() : new List<string>();
        var permissions = permissionsResult.IsSuccess ? permissionsResult.Value.Select(p => p.Name).ToList() : new List<string>();

        // Generate tokens
        var tokens = _tokenService.GenerateTokens(
            user.Id,
            user.TokenVersion,
            user.RefreshTokenVersion,
            permissions,
            roles
        );

        // Create session
        var session = new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenVersion = tokens.TokenVersion.ToString(),
            RefreshTokenVersion = tokens.RefreshTokenVersion.ToString(),
            DeviceInfo = "Registration",
            IpAddress = ipAddress,
            UserAgent = userAgent,
            LoginTime = DateTime.UtcNow,
            LastActivityTime = DateTime.UtcNow,
            ExpiresAt = tokens.RefreshTokenExpiresAt,
            IsActive = true
        };

        _context.UserSessions.Add(session);

        user.LastLoginAt = DateTime.UtcNow;
        user.LastActivityAt = DateTime.UtcNow;
        user.IsOnline = true;

        await _context.SaveChangesAsync();

        var userDto = new UserDto(
            user.Id,
            user.Email!,
            user.FirstName,
            user.LastName,
            user.DisplayName,
            user.IsOnline,
            user.LastLoginAt,
            roles,
            permissions
        );

        return Result.Success(new LoginResponse(
            tokens.AccessToken,
            tokens.RefreshToken,
            tokens.ExpiresAt,
            tokens.RefreshTokenExpiresAt,
            userDto
        ));
    }

    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request, string ipAddress, string? userAgent)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
            return Result.Failure<LoginResponse>("Invalid email or password");

        if (!user.IsActive)
            return Result.Failure<LoginResponse>("User account is disabled");

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
            return Result.Failure<LoginResponse>("Invalid email or password");

        // Check for multiple sessions
        if (!user.AllowMultipleSessions)
        {
            var activeSessions = await _context.UserSessions
                .Where(s => s.UserId == user.Id && s.IsActive && !s.IsTerminated)
                .ToListAsync();

            if (activeSessions.Count >= user.MaxConcurrentSessions)
            {
                // Terminate oldest session
                var oldestSession = activeSessions.OrderBy(s => s.LoginTime).First();
                await _sessionService.TerminateSessionAsync(user.Id, oldestSession.Id);
            }
        }

        // Get user roles and permissions
        var rolesResult = await _rolePermissionService.GetUserRolesAsync(user.Id);
        var permissionsResult = await _rolePermissionService.GetUserPermissionsAsync(user.Id);

        if (!rolesResult.IsSuccess || !permissionsResult.IsSuccess)
            return Result.Failure<LoginResponse>("Failed to get user roles/permissions");

        var roles = rolesResult.Value.Select(r => r.Name).ToList();
        var permissions = permissionsResult.Value.Select(p => p.Name).ToList();

        // Generate tokens
        var tokens = _tokenService.GenerateTokens(
            user.Id,
            user.TokenVersion,
            user.RefreshTokenVersion,
            permissions,
            roles
        );

        // Create session
        var session = new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenVersion = tokens.TokenVersion.ToString(),
            RefreshTokenVersion = tokens.RefreshTokenVersion.ToString(),
            DeviceInfo = request.DeviceInfo ?? "Unknown",
            IpAddress = ipAddress,
            UserAgent = userAgent,
            LoginTime = DateTime.UtcNow,
            LastActivityTime = DateTime.UtcNow,
            ExpiresAt = tokens.RefreshTokenExpiresAt,
            IsActive = true
        };

        _context.UserSessions.Add(session);

        // Update user activity
        user.LastLoginAt = DateTime.UtcNow;
        user.LastActivityAt = DateTime.UtcNow;
        user.IsOnline = true;

        await _context.SaveChangesAsync();

        var userDto = new UserDto(
            user.Id,
            user.Email!,
            user.FirstName,
            user.LastName,
            user.DisplayName,
            user.IsOnline,
            user.LastLoginAt,
            roles,
            permissions
        );

        return Result.Success(new LoginResponse(
            tokens.AccessToken,
            tokens.RefreshToken,
            tokens.ExpiresAt,
            tokens.RefreshTokenExpiresAt,
            userDto
        ));
    }

    public async Task<Result<LoginResponse>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var (userId, tokenId, refreshTokenVersion) = _tokenService.ValidateRefreshToken(request.RefreshToken);

        if (userId == null || refreshTokenVersion == null)
            return Result.Failure<LoginResponse>("Invalid refresh token");

        var user = await _context.Users.FindAsync(userId.Value);
        if (user == null)
            return Result.Failure<LoginResponse>("User not found");

        if (!user.IsActive)
            return Result.Failure<LoginResponse>("User account is disabled");

        if (user.RefreshTokenVersion != refreshTokenVersion)
            return Result.Failure<LoginResponse>("Refresh token has been revoked");

        // Check if token is blacklisted
        if (await _tokenService.IsTokenBlacklistedAsync(tokenId!))
            return Result.Failure<LoginResponse>("Token has been invalidated");

        // Get user roles and permissions
        var rolesResult = await _rolePermissionService.GetUserRolesAsync(user.Id);
        var permissionsResult = await _rolePermissionService.GetUserPermissionsAsync(user.Id);

        if (!rolesResult.IsSuccess || !permissionsResult.IsSuccess)
            return Result.Failure<LoginResponse>("Failed to get user roles/permissions");

        var roles = rolesResult.Value.Select(r => r.Name).ToList();
        var permissions = permissionsResult.Value.Select(p => p.Name).ToList();

        // Generate new tokens (sliding window - 10 min to 8 hours)
        var tokens = _tokenService.GenerateTokens(
            user.Id,
            user.TokenVersion,
            user.RefreshTokenVersion,
            permissions,
            roles
        );

        // Update session
        var session = await _context.UserSessions
            .Where(s => s.UserId == user.Id && s.IsActive && !s.IsTerminated)
            .OrderByDescending(s => s.LoginTime)
            .FirstOrDefaultAsync();

        if (session != null)
        {
            session.TokenVersion = tokens.TokenVersion.ToString();
            session.RefreshTokenVersion = tokens.RefreshTokenVersion.ToString();
            session.LastActivityTime = DateTime.UtcNow;
            session.ExpiresAt = tokens.RefreshTokenExpiresAt;
        }

        user.LastActivityAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var userDto = new UserDto(
            user.Id,
            user.Email!,
            user.FirstName,
            user.LastName,
            user.DisplayName,
            user.IsOnline,
            user.LastLoginAt,
            roles,
            permissions
        );

        return Result.Success(new LoginResponse(
            tokens.AccessToken,
            tokens.RefreshToken,
            tokens.ExpiresAt,
            tokens.RefreshTokenExpiresAt,
            userDto
        ));
    }

    public async Task<Result> LogoutAsync(Guid userId, LogoutRequest request, string? tokenId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return Result.Failure("User not found");

        if (request.TerminateAllSessions)
        {
            // Increment token version to invalidate all tokens
            user.TokenVersion++;
            user.RefreshTokenVersion++;

            var sessions = await _context.UserSessions
                .Where(s => s.UserId == userId && s.IsActive && !s.IsTerminated)
                .ToListAsync();

            foreach (var session in sessions)
            {
                session.IsActive = false;
                session.IsTerminated = true;
                session.TerminationReason = "User logged out from all sessions";
            }
        }
        else if (!string.IsNullOrEmpty(tokenId))
        {
            // Blacklist current token
            await _tokenService.InvalidateTokenAsync(tokenId, userId, "User logged out");

            // Terminate current session
            var session = await _context.UserSessions
                .Where(s => s.UserId == userId && s.TokenVersion == user.TokenVersion.ToString())
                .FirstOrDefaultAsync();

            if (session != null)
            {
                session.IsActive = false;
                session.IsTerminated = true;
                session.TerminationReason = "User logged out";
            }
        }

        // Check if user has any active sessions
        var hasActiveSessions = await _context.UserSessions
            .AnyAsync(s => s.UserId == userId && s.IsActive && !s.IsTerminated);

        user.IsOnline = hasActiveSessions;

        await _context.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> ValidateTokenAsync(string token)
    {
        var (userId, tokenId, tokenVersion) = _tokenService.ValidateAccessToken(token);

        if (userId == null || tokenId == null || tokenVersion == null)
            return Result.Failure("Invalid token");

        if (await _tokenService.IsTokenBlacklistedAsync(tokenId))
            return Result.Failure("Token has been invalidated");

        var user = await _context.Users.FindAsync(userId.Value);
        if (user == null)
            return Result.Failure("User not found");

        if (!user.IsActive)
            return Result.Failure("User account is disabled");

        if (user.TokenVersion != tokenVersion)
            return Result.Failure("Token has been revoked");

        return Result.Success();
    }
}





//public class AuthService : IAuthService
//{
//    private readonly UserManager<ApplicationUser> _um;
//    private readonly IConfiguration _cfg;
//    public AuthService(UserManager<ApplicationUser> um, IConfiguration cfg)
//    {
//        _um = um;
//        _cfg = cfg;
//    }

//    public async Task<string> RegisterAsync(RegisterDto dto)
//    {
//        var user = new ApplicationUser { UserName = dto.Email, Email = dto.Email, FirstName = dto.FirstName, LastName = dto.LastName };
//        IdentityResult res = await _um.CreateAsync(user, dto.Password);
//        if (!res.Succeeded)
//        {
//            throw new InvalidOperationException(string.Join(";", res.Errors.Select(e => e.Description)));
//        }

//        // Optionally assign roles/claims
//        await _um.AddClaimAsync(user, new Claim("role", "user"));

//        return await GenerateTokenAsync(user);
//    }

//    public async Task<string> LoginAsync(LoginDto dto)
//    {
//        ApplicationUser? user = await _um.FindByEmailAsync(dto.Email);
//        if (user == null)
//        {
//            throw new InvalidOperationException("Invalid login");
//        }

//        if (!await _um.CheckPasswordAsync(user, dto.Password))
//        {
//            throw new InvalidOperationException("Invalid login");
//        }

//        return await GenerateTokenAsync(user);
//    }

//    private async Task<string> GenerateTokenAsync(ApplicationUser user)
//    {
//        string key = _cfg["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key missing");
//        string issuer = _cfg["Jwt:Issuer"] ?? "store";
//        string audience = _cfg["Jwt:Audience"] ?? "store";
//        var claims = new List<Claim>
//        {
//            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
//            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
//            new Claim("displayName", user.DisplayName ?? string.Empty)
//        };

//        IList<Claim> userClaims = await _um.GetClaimsAsync(user);
//        claims.AddRange(userClaims);

//        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
//        var creds = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256);
//        var token = new JwtSecurityToken(issuer, audience, claims, expires: DateTime.UtcNow.AddDays(7), signingCredentials: creds);
//        return new JwtSecurityTokenHandler().WriteToken(token);
//    }
//}



