namespace LMS.Application.Features.Auth.DTOs;

public class GenerateTokenRequest
{
    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}