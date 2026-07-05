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
        UpdateAssignmentRequest request);

    Task<ServiceResponse> DeleteAssignment(int assignmentId);
}
