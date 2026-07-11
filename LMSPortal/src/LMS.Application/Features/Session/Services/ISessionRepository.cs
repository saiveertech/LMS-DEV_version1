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

    // Expires the session if it has gone idle too long, then (if still
    // active) marks this call as activity. Returns whether it's still active.
    Task<bool> ValidateAndTouchSessionAsync(string sessionId);

    // Background sweep: expires every session past its idle timeout or
    // absolute JWT expiry. Returns how many rows were expired.
    Task<int> ExpireStaleSessionsAsync();

    // Deactivates any active session(s) for this user without error, even
    // if none exist. For flows that must never be blocked by the
    // single-device gate (e.g. bootstrap/testing token issuance).
    Task DeactivateAllSessionsForUserAsync(string userId);
}
