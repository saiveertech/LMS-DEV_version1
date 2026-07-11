using Microsoft.AspNetCore.Http;

namespace LMS.Application.Features.Course.DTOs;

public class CreateCourseRequest
{
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public List<CourseSlideRequest>? Slides { get; set; }

    public int CompletionTimeSeconds { get; set; }

    public string? Tags { get; set; }

    public IFormFile? IntroVideo { get; set; }

    public IFormFile? CourseIcon { get; set; }

    public string? CreatedById { get; set; }

    public string? CreatedByName { get; set; }

    public string? CreatedByRole { get; set; }
}

public class UpdateCourseRequest
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public List<CourseSlideRequest>? Slides { get; set; }

    public int? CompletionTimeSeconds { get; set; }

    public string? Tags { get; set; }

    public string? CourseStatus { get; set; }

    public bool? IsActive { get; set; }

    public IFormFile? IntroVideo { get; set; }

    public IFormFile? CourseIcon { get; set; }

    public string? EditedById { get; set; }

    public string? EditedByName { get; set; }

    public string? EditedByRole { get; set; }
}
