using LMS.Application.Features.Assignment.DTOs;

namespace LMS.Application.Features.Assignment.Services;

public interface IAssignmentRepository
{
    Task<object> CreateAssignment(
        CreateAssignmentRequest request,
        string introVideoUrl,
        string questionsCsvUrl,
        string assessmentIconUrl,
        string createdById,
        string createdByName,
        string createdByRole);

    Task<object?> GetAssignments(int? assignmentId = null);

    Task<bool> UpdateAssignment(
        int assignmentId,
        UpdateAssignmentRequest request,
        string? introVideoUrl,
        string? questionsCsvUrl,
        string? assessmentIconUrl,
        string editedById,
        string editedByName,
        string editedByRole);

    Task<bool> DeleteAssignment(
        int assignmentId,
        string deletedById,
        string deletedByName,
        string deletedByRole);

    // Replaces the whole question bank for this assignment. Returns how many
    // questions were imported.
    Task<int> ReplaceQuestions(int assignmentId, List<QuestionImportRow> questions);

    Task<List<AssignmentQuestionResponse>> GetQuestions(int assignmentId);
}
