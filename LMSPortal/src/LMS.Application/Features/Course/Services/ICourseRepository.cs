using LMS.Application.Features.Course.DTOs;

namespace LMS.Application.Features.Course.Services;

public interface ICourseRepository
{
    Task<object> RegisterCourse(
        CreateCourseRequest request,
        string introVideoUrl,
        string courseIconUrl,
        string createdBy,
        string createdByRole);

    Task<object?> GetCourseById(int? courseId = null);

    Task<bool> UpdateCourse(
        int courseId,
        UpdateCourseRequest request,
        string? introVideoUrl,
        string? courseIconUrl);
}
