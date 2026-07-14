namespace LMS.Application.Features.Auth.DTOs;

// ─── Request ────────────────────────────────────────────────────────────────

public class EnrollAssignmentRequest
{
    /// <summary>The unique identifier of the student being enrolled.</summary>
    public string StudentId { get; set; } = string.Empty;

    /// <summary>The numeric ID of the assignment to enroll in.</summary>
    public int AssignmentId { get; set; }
}

public class SubmitAssignmentAttemptRequest
{
    public string StudentId { get; set; } = string.Empty;

    public int AssignmentId { get; set; }

    /// <summary>This attempt's score (0-100). AssessmentScore stored on the
    /// enrollment keeps the best score across all attempts.</summary>
    public decimal Score { get; set; }
}

public class AnswerSubmission
{
    public int QuestionId { get; set; }

    /// <summary>A | B | C | D</summary>
    public string SelectedOption { get; set; } = string.Empty;
}

public class SubmitAssignmentAnswersRequest
{
    public string StudentId { get; set; } = string.Empty;

    public int AssignmentId { get; set; }

    public List<AnswerSubmission> Answers { get; set; } = new();
}

public class AssignmentAnswerResultItem
{
    public int QuestionId { get; set; }

    public string? SelectedOption { get; set; }

    public string CorrectOption { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }
}

public class SubmitAssignmentAnswersResponse
{
    public int AssignmentId { get; set; }

    public int AttemptNumber { get; set; }

    public int CorrectCount { get; set; }

    public int TotalQuestions { get; set; }

    public decimal CorrectMarks { get; set; }

    public decimal TotalMarks { get; set; }

    /// <summary>correctMarks / totalMarks * 100 — the score fed into the existing attempt/pass/certificate flow.</summary>
    public decimal ScorePercentage { get; set; }

    public bool Passed { get; set; }

    public List<AssignmentAnswerResultItem> Results { get; set; } = new();
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
