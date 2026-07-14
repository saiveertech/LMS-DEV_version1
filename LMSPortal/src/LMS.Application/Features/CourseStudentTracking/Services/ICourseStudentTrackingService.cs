using LMS.Application.Common;
using LMS.Application.Features.CourseStudentTracking.DTOs;

namespace LMS.Application.Features.CourseStudentTracking.Services;

public interface ICourseStudentTrackingService
{
    Task<List<CourseStudentTrackingResponse>> GetCourseStudents(
        int courseId,
        CourseStudentTrackingFilter filter);

    Task<ServiceResponse> UpdateCourseStudentProgress(
        int courseId,
        string studentId,
        UpdateCourseProgressRequest request);

    Task<List<AssignmentStudentTrackingResponse>> GetAssignmentStudents(
        int assignmentId,
        CourseStudentTrackingFilter filter);
}
