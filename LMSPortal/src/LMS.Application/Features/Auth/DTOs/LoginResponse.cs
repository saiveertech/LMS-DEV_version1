namespace LMS.Application.Features.Auth.DTOs;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
}