namespace LMS.Application.Features.Admin.DTOs;

public class RegisterAdminRequest
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public int? ExperienceYears { get; set; }

    public string? Skills { get; set; }

    public string? Bio { get; set; }
}

public class UpdateAdminRequest
{
    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public int? ExperienceYears { get; set; }

    public string? Skills { get; set; }

    public string? Bio { get; set; }
}