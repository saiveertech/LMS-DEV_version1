namespace LMS.Application.Features.Assignment.DTOs;

// Internal/trainer view — includes the correct answer.
public class AssignmentQuestionResponse
{
    public int QuestionId { get; set; }

    public int AssignmentId { get; set; }

    public string QuestionText { get; set; } = string.Empty;

    public string OptionA { get; set; } = string.Empty;

    public string OptionB { get; set; } = string.Empty;

    public string OptionC { get; set; } = string.Empty;

    public string OptionD { get; set; } = string.Empty;

    public string CorrectOption { get; set; } = string.Empty;

    public decimal Marks { get; set; }

    public int SortOrder { get; set; }
}

// Student-facing view — CorrectOption deliberately omitted.
public class AssignmentQuestionForStudentResponse
{
    public int QuestionId { get; set; }

    public string QuestionText { get; set; } = string.Empty;

    public string OptionA { get; set; } = string.Empty;

    public string OptionB { get; set; } = string.Empty;

    public string OptionC { get; set; } = string.Empty;

    public string OptionD { get; set; } = string.Empty;

    public decimal Marks { get; set; }

    public int SortOrder { get; set; }
}

// Parsed from the trainer's uploaded CSV, before it has an Id.
public class QuestionImportRow
{
    public string QuestionText { get; set; } = string.Empty;

    public string OptionA { get; set; } = string.Empty;

    public string OptionB { get; set; } = string.Empty;

    public string OptionC { get; set; } = string.Empty;

    public string OptionD { get; set; } = string.Empty;

    public string CorrectOption { get; set; } = string.Empty;

    public decimal Marks { get; set; } = 1;
}
