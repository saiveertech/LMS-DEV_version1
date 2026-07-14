namespace LMS.Application.Features.CourseStudentTracking.DTOs;

public class AssignmentStudentTrackingResponse
{
    public string StudentId { get; set; } = string.Empty;

    public string StudentName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public int AssignmentId { get; set; }

    public string AssignmentTitle { get; set; } = string.Empty;

    public DateTime EnrollmentDate { get; set; }

    /// <summary>Self | TrainerAssigned</summary>
    public string EnrollmentSource { get; set; } = string.Empty;

    /// <summary>Set only when EnrollmentSource is TrainerAssigned.</summary>
    public string? AssignedByName { get; set; }

    public string AssignmentStatus { get; set; } = string.Empty;

    public decimal? AssessmentScore { get; set; }

    public decimal PassPercentage { get; set; }

    public int Attempts { get; set; }

    public bool CertificateGenerated { get; set; }

    public string? CertificateId { get; set; }

    public DateTime? CertificateIssueDate { get; set; }

    public string? CertificateUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
