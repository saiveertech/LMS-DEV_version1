using LMS.Application.Features.CourseStudentTracking.DTOs;

namespace LMS.Application.Features.CourseStudentTracking.Services;

public interface ICourseStudentTrackingRepository
{
    Task<List<CourseStudentTrackingResponse>> GetCourseStudents(
        int courseId,
        CourseStudentTrackingFilter filter);

    // Throws if the enrollment doesn't exist or CompletedLessons is invalid.
    Task<CourseStudentTrackingResponse?> UpdateCourseStudentProgress(
        int courseId,
        string studentId,
        UpdateCourseProgressRequest request);

    Task<List<AssignmentStudentTrackingResponse>> GetAssignmentStudents(
        int assignmentId,
        CourseStudentTrackingFilter filter);
}
