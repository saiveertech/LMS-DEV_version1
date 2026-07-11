using LMS.Application.Common;

namespace LMS.Application.Features.Session.Services;

public class SessionService : ISessionService
{
    private readonly ISessionRepository _repo;

    public SessionService(ISessionRepository repo)
    {
        _repo = repo;
    }

    public async Task<(bool Success, string Message, string? SessionId)> CreateSessionAsync(
        string userId,
        string role,
        string? deviceInfo)
    {
        var sessionId = Guid.NewGuid().ToString("N");

        try
        {
            await _repo.CreateSessionAsync(sessionId, userId, role, deviceInfo);

            return (true, "Session created successfully.", sessionId);
        }
        catch (Exception ex)
        {
            return (false, ex.Message, null);
        }
    }

    public async Task<string> CreateOrReplaceSessionAsync(
        string userId,
        string role,
        string? deviceInfo)
    {
        await _repo.DeactivateAllSessionsForUserAsync(userId);

        var sessionId = Guid.NewGuid().ToString("N");

        await _repo.CreateSessionAsync(sessionId, userId, role, deviceInfo);

        return sessionId;
    }

    public async Task<ServiceResponse> GetCurrentSessionAsync(string sessionId)
    {
        var session = await _repo.GetSessionBySessionIdAsync(sessionId);

        if (session == null || !session.IsActive)
        {
            return new ServiceResponse
            {
                Success = false,
                Message = "No active session found."
            };
        }

        return new ServiceResponse
        {
            Success = true,
            Message = "Current session retrieved successfully.",
            Data = session
        };
    }

    public async Task<ServiceResponse> TerminateSessionAsync(string sessionId)
    {
        try
        {
            await _repo.TerminateSessionAsync(sessionId);

            return new ServiceResponse
            {
                Success = true,
                Message = "Session terminated successfully."
            };
        }
        catch (Exception ex)
        {
            return new ServiceResponse
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    public async Task<bool> ValidateAndTouchSessionAsync(string sessionId)
    {
        return await _repo.ValidateAndTouchSessionAsync(sessionId);
    }

    public async Task<int> ExpireStaleSessionsAsync()
    {
        return await _repo.ExpireStaleSessionsAsync();
    }
}
