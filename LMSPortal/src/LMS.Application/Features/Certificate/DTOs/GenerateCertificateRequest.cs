namespace LMS.Application.Features.Certificate.DTOs;

public class GenerateCertificateRequest
{
    public string StudentId { get; set; } = string.Empty;

    public int CourseId { get; set; }

    public decimal CompletionPercentage { get; set; }

    public decimal AssessmentScore { get; set; }

    public DateTime CompletionDate { get; set; }

    // Optional: override from request body, otherwise extracted from JWT claims
    public string? CreatedById { get; set; }

    public string? CreatedByName { get; set; }

    public string? CreatedByRole { get; set; }
}
