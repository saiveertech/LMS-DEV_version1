namespace LMS.Application.Features.Session.DTOs;

public class SessionResponse
{
    public int Id { get; set; }

    public string SessionId { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public string? DeviceInfo { get; set; }

    public DateTime LoginTime { get; set; }

    public DateTime? LogoutTime { get; set; }

    public bool IsActive { get; set; }
}
