using LMS.Application.Features.CourseStudentTracking.DTOs;

namespace LMS.Application.Features.CourseStudentTracking.Services;

public class CourseStudentTrackingService : ICourseStudentTrackingService
{
    private readonly ICourseStudentTrackingRepository _repository;

    public CourseStudentTrackingService(ICourseStudentTrackingRepository repository)
    {
        _repository = repository;
    }

    public Task<List<CourseStudentTrackingResponse>> GetCourseStudents(
        int courseId,
        CourseStudentTrackingFilter filter)
    {
        return _repository.GetCourseStudents(courseId, filter);
    }
}
