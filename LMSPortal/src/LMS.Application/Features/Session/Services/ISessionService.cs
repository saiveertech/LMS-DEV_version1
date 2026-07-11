using LMS.Application.Common;

namespace LMS.Application.Features.Session.Services;

public interface ISessionService
{
    Task<(bool Success, string Message, string? SessionId)> CreateSessionAsync(
        string userId,
        string role,
        string? deviceInfo);

    // Never blocked by the single-device gate: clears any existing active
    // session for this user first, then always creates a fresh one. For
    // flows that must always succeed (e.g. bootstrap/testing token issuance).
    Task<string> CreateOrReplaceSessionAsync(
        string userId,
        string role,
        string? deviceInfo);

    Task<ServiceResponse> GetCurrentSessionAsync(string sessionId);

    Task<ServiceResponse> TerminateSessionAsync(string sessionId);

    // Expires the session if idle too long, otherwise records this call as
    // activity. Used both by the per-request JWT validation hook and by the
    // explicit heartbeat endpoint. Returns whether the session is still active.
    Task<bool> ValidateAndTouchSessionAsync(string sessionId);

    // Background sweep across all sessions — see ISessionRepository.ExpireStaleSessionsAsync.
    Task<int> ExpireStaleSessionsAsync();
}
