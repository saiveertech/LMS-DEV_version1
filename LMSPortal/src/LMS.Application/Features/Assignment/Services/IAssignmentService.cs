using LMS.Application.Common;
using LMS.Application.Features.Assignment.DTOs;

namespace LMS.Application.Features.Assignment.Services;

public interface IAssignmentService
{
    Task<ServiceResponse> CreateAssignment(
        CreateAssignmentRequest request,
        string createdById,
        string createdByName,
        string createdByRole);

    Task<object?> GetAssignments(int? assignmentId = null);

    Task<ServiceResponse> UpdateAssignment(
        int assignmentId,
        UpdateAssignmentRequest request,
        string editedById,
        string editedByName,
        string editedByRole);

    Task<ServiceResponse> DeleteAssignment(
        int assignmentId,
        string deletedById,
        string deletedByName,
        string deletedByRole);

    // Trainer/admin view — includes CorrectOption.
    Task<List<AssignmentQuestionResponse>> GetQuestions(int assignmentId);

    // Student view — CorrectOption stripped out.
    Task<List<AssignmentQuestionForStudentResponse>> GetQuestionsForStudent(int assignmentId);
}
