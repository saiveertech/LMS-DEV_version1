namespace LMS.Application.Features.Course.DTOs;

public class CourseResponse
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? IntroVideoUrl { get; set; }

    public string? SlidesJson { get; set; }

    public int CompletionTimeSeconds { get; set; }

    public decimal PassPercentage { get; set; }

    public string? WwEnvClientId { get; set; }

    public string? CourseIconUrl { get; set; }

    public string? Tags { get; set; }

    public string CourseStatus { get; set; } = string.Empty;

    public string CreatedById { get; set; } = string.Empty;

    public string CreatedByName { get; set; } = string.Empty;

    public string CreatedByRole { get; set; } = string.Empty;

    public string? EditedById { get; set; }

    public string? EditedByName { get; set; }

    public string? EditedByRole { get; set; }

    public string? DeletedById { get; set; }

    public string? DeletedByName { get; set; }

    public string? DeletedByRole { get; set; }

    public DateTime? DeletedAt { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
