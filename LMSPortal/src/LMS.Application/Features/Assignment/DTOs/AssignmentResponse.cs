namespace LMS.Application.Features.Assignment.DTOs;

public class AssignmentResponse
{
    public int AssignmentId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? IntroVideoUrl { get; set; }

    public string? QuestionsCsvUrl { get; set; }

    public int CompletionTimeSeconds { get; set; }

    public decimal PassPercentage { get; set; }

    public string? WwEnvClientId { get; set; }

    public string? AssessmentIconUrl { get; set; }

    public string? Tags { get; set; }

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

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
