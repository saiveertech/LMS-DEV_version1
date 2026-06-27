namespace LMS.Application.Features.Auth.DTOs;

public class RegisterTrainerRequest
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public int ExperienceYears { get; set; }

    public string? CurrentCompany { get; set; }

    public string? Designation { get; set; }

    public string? Bio { get; set; }

    public string? LinkedInUrl { get; set; }
}
public class UpdateTrainerRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public int? ExperienceYears { get; set; }
    public string? CurrentCompany { get; set; }
    public string? Designation { get; set; }
    public string? Bio { get; set; }
    public string? LinkedInUrl { get; set; }
}