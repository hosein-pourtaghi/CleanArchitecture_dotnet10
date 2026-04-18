using Application.Common.DTOs.Identities;
using Application.Common.Interfaces.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Infrastructure.Services.Identities;

public class SessionService : ISessionService
{
    private readonly IApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SessionService(IApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result<OnlineUsersResponse>> GetOnlineUsersAsync(int page = 1, int pageSize = 20)
    {
        var query = _context.UserSessions
            .Include(s => s.User)
            .Where(s => s.IsActive && !s.IsTerminated && s.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(s => s.LastActivityTime);

        var totalCount = await query.CountAsync();

        var sessions = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new SessionDto(
                s.Id,
                s.UserId,
                s.User.Email!,
                s.DeviceInfo,
                s.IpAddress,
                s.LoginTime,
                s.LastActivityTime,
                s.ExpiresAt,
                s.IsActive
            ))
            .ToListAsync();

        return Result.Success(new OnlineUsersResponse(totalCount, sessions));
    }

    public async Task<Result<List<SessionDto>>> GetUserSessionsAsync(Guid userId)
    {
        var sessions = await _context.UserSessions
            .Include(s => s.User)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.LoginTime)
            .Select(s => new SessionDto(
                s.Id,
                s.UserId,
                s.User.Email!,
                s.DeviceInfo,
                s.IpAddress,
                s.LoginTime,
                s.LastActivityTime,
                s.ExpiresAt,
                s.IsActive
            ))
            .ToListAsync();

        return Result.Success(sessions);
    }

    public async Task<Result> TerminateSessionAsync(Guid userId, Guid sessionId)
    {
        var session = await _context.UserSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId);

        if (session == null)
            return Result.Failure("Session not found");

        session.IsActive = false;
        session.IsTerminated = true;
        session.TerminationReason = "Terminated by user or administrator";

        // Check if user has other active sessions
        var hasActiveSessions = await _context.UserSessions
            .AnyAsync(s => s.UserId == userId && s.IsActive && s.Id != sessionId);

        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            user.IsOnline = hasActiveSessions;
        }

        await _context.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> TerminateAllSessionsAsync(Guid userId, Guid? exceptSessionId = null)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return Result.Failure("User not found");

        // Increment token version to invalidate all tokens
        user.TokenVersion++;
        user.RefreshTokenVersion++;

        var sessionsQuery = _context.UserSessions
            .Where(s => s.UserId == userId && s.IsActive && !s.IsTerminated);

        if (exceptSessionId.HasValue)
        {
            sessionsQuery = sessionsQuery.Where(s => s.Id != exceptSessionId.Value);
        }

        var sessions = await sessionsQuery.ToListAsync();

        foreach (var session in sessions)
        {
            session.IsActive = false;
            session.IsTerminated = true;
            session.TerminationReason = "Terminated by administrator";
        }

        user.IsOnline = false;

        await _context.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> TerminateExpiredSessionsAsync()
    {
        var expiredSessions = await _context.UserSessions
            .Where(s => s.IsActive && s.ExpiresAt < DateTime.UtcNow)
            .ToListAsync();

        foreach (var session in expiredSessions)
        {
            session.IsActive = false;
            session.IsTerminated = true;
            session.TerminationReason = "Session expired";
        }

        // Update users' online status
        var userIds = expiredSessions.Select(s => s.UserId).Distinct();
        foreach (var userId in userIds)
        {
            var hasActiveSessions = await _context.UserSessions
                .AnyAsync(s => s.UserId == userId && s.IsActive && !s.IsTerminated);

            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.IsOnline = hasActiveSessions;
            }
        }

        await _context.SaveChangesAsync();

        return Result.Success();
    }

    public async Task UpdateSessionActivityAsync(Guid sessionId)
    {
        var session = await _context.UserSessions.FindAsync(sessionId);
        if (session != null)
        {
            session.LastActivityTime = DateTime.UtcNow;

            var user = await _context.Users.FindAsync(session.UserId);
            if (user != null)
            {
                user.LastActivityAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }
    }
}
