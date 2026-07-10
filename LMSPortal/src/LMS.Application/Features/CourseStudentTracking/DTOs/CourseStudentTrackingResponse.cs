namespace LMS.Application.Features.CourseStudentTracking.DTOs;

public class CourseStudentTrackingResponse
{
    public string StudentId { get; set; } = string.Empty;

    public string StudentName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public int CourseId { get; set; }

    public string CourseTitle { get; set; } = string.Empty;

    public DateTime EnrollmentDate { get; set; }

    public string RegistrationStatus { get; set; } = string.Empty;

    public string CourseStatus { get; set; } = string.Empty;

    public decimal CompletionPercentage { get; set; }

    public int CompletedLessons { get; set; }

    public int TotalLessons { get; set; }

    public decimal? AssessmentScore { get; set; }

    public decimal PassPercentage { get; set; }

    public bool IsPassed { get; set; }

    public bool CertificateGenerated { get; set; }

    public string? CertificateId { get; set; }

    public DateTime? CertificateIssueDate { get; set; }

    public string? CertificateUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
