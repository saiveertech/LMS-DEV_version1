namespace LMS.Application.Features.Certificate.DTOs;

public class CertificateResponse
{
    public int Id { get; set; }

    public string CertificateId { get; set; } = string.Empty;

    public string CredentialId { get; set; } = string.Empty;

    /// <summary>Course | Assignment — which completion path issued this certificate.</summary>
    public string CertificateType { get; set; } = string.Empty;

    public string StudentId { get; set; } = string.Empty;

    public string StudentName { get; set; } = string.Empty;

    public string StudentEmail { get; set; } = string.Empty;

    public int? CourseId { get; set; }

    public string? CourseName { get; set; }

    public int? AssignmentId { get; set; }

    public decimal? CompletionPercentage { get; set; }

    public decimal? AssessmentScore { get; set; }

    public decimal? PassPercentage { get; set; }

    public DateTime CompletionDate { get; set; }

    public DateTime IssuedDate { get; set; }

    public string CertificateUrl { get; set; } = string.Empty;

    public bool IsValid { get; set; }

    public string CreatedById { get; set; } = string.Empty;

    public string CreatedByName { get; set; } = string.Empty;

    public string CreatedByRole { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
