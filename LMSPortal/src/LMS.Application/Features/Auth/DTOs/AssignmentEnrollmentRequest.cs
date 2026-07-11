namespace LMS.Application.Features.Auth.DTOs;

// ─── Request ────────────────────────────────────────────────────────────────

public class EnrollAssignmentRequest
{
    /// <summary>The unique identifier of the student being enrolled.</summary>
    public string StudentId { get; set; } = string.Empty;

    /// <summary>The numeric ID of the assignment to enroll in.</summary>
    public int AssignmentId { get; set; }
}

// ─── Response ────────────────────────────────────────────────────────────────

public class EnrolledAssignmentResponse
{
    public string EnrollmentId { get; set; } = string.Empty;

    public string StudentId { get; set; } = string.Empty;

    public int AssignmentId { get; set; }

    public string AssignmentTitle { get; set; } = string.Empty;

    public string? AssignmentDescription { get; set; }

    public string? AssessmentIconUrl { get; set; }

    public string? Tags { get; set; }

    /// <summary>Assignment duration in seconds — sourced from the Assignments table.</summary>
    public int CompletionTimeSeconds { get; set; }

    public decimal PassPercentage { get; set; }

    public DateTime EnrollmentDate { get; set; }

    /// <summary>Enrolled | In Progress | Completed</summary>
    public string AssignmentStatus { get; set; } = string.Empty;

    public decimal? AssessmentScore { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
