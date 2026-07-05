using LMS.Application.Common;
using LMS.Application.Features.Course.DTOs;

namespace LMS.Application.Features.Course.Services;

public interface ICourseService
{
    Task<ServiceResponse> RegisterCourse(
        CreateCourseRequest request,
        string createdById,
        string createdByName,
        string createdByRole);

    Task<object?> GetCourseById(int? courseId = null);

    Task<ServiceResponse> UpdateCourse(
        int courseId,
        UpdateCourseRequest request);
}
