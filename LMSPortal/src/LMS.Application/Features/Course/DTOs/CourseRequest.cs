using Microsoft.AspNetCore.Http;

namespace LMS.Application.Features.Course.DTOs;

public class CreateCourseRequest
{
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? SlidesJson { get; set; }

    public int CompletionTimeSeconds { get; set; }

    public decimal PassPercentage { get; set; }

    public string? WwEnvClientId { get; set; }

    public string? Tags { get; set; }

    public IFormFile? IntroVideo { get; set; }

    public IFormFile? CourseIcon { get; set; }
}
