namespace LMS.Application.Features.Auth.DTOs;

public class RegisterStudentRequest
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string EducationDetails { get; set; } = string.Empty;

    public string AreaOfInterest { get; set; } = string.Empty;
}