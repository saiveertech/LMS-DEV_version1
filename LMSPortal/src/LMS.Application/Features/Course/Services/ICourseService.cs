using LMS.Application.Common;
using LMS.Application.Features.Course.DTOs;

namespace LMS.Application.Features.Course.Services;

public interface ICourseService
{
    Task<ServiceResponse> RegisterCourse(
        CreateCourseRequest request,
        string createdBy,
        string createdByRole);

    Task<object?> GetCourseById(int courseId);
}
