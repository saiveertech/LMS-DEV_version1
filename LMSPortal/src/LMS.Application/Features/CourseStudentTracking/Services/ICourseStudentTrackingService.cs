using LMS.Application.Features.CourseStudentTracking.DTOs;

namespace LMS.Application.Features.CourseStudentTracking.Services;

public interface ICourseStudentTrackingService
{
    Task<List<CourseStudentTrackingResponse>> GetCourseStudents(
        int courseId,
        CourseStudentTrackingFilter filter);
}
