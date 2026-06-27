namespace LMS.Application.Features.Auth.DTOs;

public class ResetPasswordRequest
{
    public string Email { get; set; } = string.Empty;

    public string OldPassword { get; set; } = string.Empty;

    public string NewPassword { get; set; } = string.Empty;
}