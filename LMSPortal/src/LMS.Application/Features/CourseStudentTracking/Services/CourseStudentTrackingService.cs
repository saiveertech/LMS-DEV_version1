using LMS.Application.Common;
using LMS.Application.Features.Certificate.Services;
using LMS.Application.Features.CourseStudentTracking.DTOs;

namespace LMS.Application.Features.CourseStudentTracking.Services;

public class CourseStudentTrackingService : ICourseStudentTrackingService
{
    private readonly ICourseStudentTrackingRepository _repository;
    private readonly ICertificateService _certificateService;

    public CourseStudentTrackingService(
        ICourseStudentTrackingRepository repository,
        ICertificateService certificateService)
    {
        _repository = repository;
        _certificateService = certificateService;
    }

    public Task<List<CourseStudentTrackingResponse>> GetCourseStudents(
        int courseId,
        CourseStudentTrackingFilter filter)
    {
        return _repository.GetCourseStudents(courseId, filter);
    }

    public async Task<ServiceResponse> UpdateCourseStudentProgress(
        int courseId,
        string studentId,
        UpdateCourseProgressRequest request)
    {
        if (courseId <= 0)
        {
            return new ServiceResponse
            {
                Success = false,
                Message = "A valid Course ID is required."
            };
        }

        if (string.IsNullOrWhiteSpace(studentId))
        {
            return new ServiceResponse
            {
                Success = false,
                Message = "Student ID is required."
            };
        }

        if (request.CompletedLessons < 0)
        {
            return new ServiceResponse
            {
                Success = false,
                Message = "CompletedLessons cannot be negative."
            };
        }

        if (request.TotalLessons.HasValue && request.CompletedLessons > request.TotalLessons.Value)
        {
            return new ServiceResponse
            {
                Success = false,
                Message = "CompletedLessons cannot exceed TotalLessons."
            };
        }

        try
        {
            var result = await _repository.UpdateCourseStudentProgress(courseId, studentId, request);

            if (result == null)
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "Enrollment not found for this student and course."
                };
            }

            var message = "Progress updated successfully.";

            // Auto-issue the course certificate the instant completion hits
            // 100% — no manual trainer/admin trigger. Idempotent: a repeat
            // update after completion just fails the "already exists" check
            // inside GenerateCourseCertificateAsync and is ignored here.
            if (result.CourseStatus == "Completed")
            {
                var certResult = await _certificateService.GenerateCourseCertificateAsync(
                    studentId,
                    courseId,
                    result.CompletionPercentage,
                    DateTime.UtcNow,
                    "system",
                    "System",
                    "System");

                message = certResult.Success
                    ? "Progress updated — course marked as Completed and certificate issued."
                    : "Progress updated — course marked as Completed.";
            }

            return new ServiceResponse
            {
                Success = true,
                Message = message,
                Data = result
            };
        }
        catch (Exception ex)
        {
            return new ServiceResponse
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    public Task<List<AssignmentStudentTrackingResponse>> GetAssignmentStudents(
        int assignmentId,
        CourseStudentTrackingFilter filter)
    {
        return _repository.GetAssignmentStudents(assignmentId, filter);
    }
}
