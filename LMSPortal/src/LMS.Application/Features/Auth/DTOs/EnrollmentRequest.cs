namespace LMS.Application.Features.Auth.DTOs;

// ─── Request ────────────────────────────────────────────────────────────────

public class EnrollCourseRequest
{
    /// <summary>The unique identifier of the student being enrolled.</summary>
    public string StudentId { get; set; } = string.Empty;

    /// <summary>The numeric ID of the course to enroll in.</summary>
    public int CourseId { get; set; }
}

// ─── Response ────────────────────────────────────────────────────────────────

public class EnrolledCourseResponse
{
    public string EnrollmentId { get; set; } = string.Empty;

    public string StudentId { get; set; } = string.Empty;

    public int CourseId { get; set; }

    public string CourseTitle { get; set; } = string.Empty;

    public string? CourseDescription { get; set; }

    public string? CourseIconUrl { get; set; }

    public string? Tags { get; set; }

    /// <summary>Course duration in seconds — sourced from the Courses table.</summary>
    public int CompletionTimeSeconds { get; set; }

    public decimal PassPercentage { get; set; }

    public DateTime EnrollmentDate { get; set; }

    /// <summary>Enrolled | In Progress | Completed</summary>
    public string CourseStatus { get; set; } = string.Empty;

    /// <summary>Pending | Issued</summary>
    public string CertificateStatus { get; set; } = string.Empty;

    public DateTime? CertificateIssueDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
