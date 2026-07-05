using LMS.Application.Features.Course.DTOs;

namespace LMS.Application.Features.Course.Services;

public interface ICourseRepository
{
    Task<object> RegisterCourse(
        CreateCourseRequest request,
        string introVideoUrl,
        string courseIconUrl,
        string createdById,
        string createdByName,
        string createdByRole);

    Task<object?> GetCourseById(int? courseId = null);

    Task<bool> UpdateCourse(
        int courseId,
        UpdateCourseRequest request,
        string? introVideoUrl,
        string? courseIconUrl,
        string editedById,
        string editedByName,
        string editedByRole);

    Task<bool> DeleteCourse(
        int courseId,
        string deletedById,
        string deletedByName,
        string deletedByRole);
}
