using LMS.Application.Common;

namespace LMS.Application.Features.Session.Services;

public interface ISessionService
{
    Task<(bool Success, string Message, string? SessionId)> CreateSessionAsync(
        string userId,
        string role,
        string? deviceInfo);

    Task<ServiceResponse> GetCurrentSessionAsync(string sessionId);

    Task<ServiceResponse> TerminateSessionAsync(string sessionId);

    Task<bool> IsSessionActiveAsync(string sessionId);
}
