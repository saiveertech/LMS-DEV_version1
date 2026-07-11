using LMS.Application.Features.Session.DTOs;

namespace LMS.Application.Features.Session.Services;

public interface ISessionRepository
{
    // Throws if the user already has an active session (single-device gate).
    Task CreateSessionAsync(
        string sessionId,
        string userId,
        string role,
        string? deviceInfo);

    Task<SessionResponse?> GetSessionBySessionIdAsync(string sessionId);

    // Throws if the session does not exist or is already inactive.
    Task TerminateSessionAsync(string sessionId);
}
