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
        string? assessmentIconUrl);

    Task<bool> DeleteAssignment(int assignmentId);
}
