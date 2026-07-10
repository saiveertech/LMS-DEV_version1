using LMS.Application.Features.Auth.DTOs;

namespace LMS.Application.Features.Auth.Services.Student;

public interface IStudentRepository
{
    Task<object> RegisterStudent(RegisterStudentRequest request);
    Task<object?> GetStudentById(string? studentId = null);
    Task<bool> UpdateStudent(string studentId, UpdateStudentRequest request);

    Task<object> EnrollCourse(EnrollCourseRequest request);
    Task<object?> GetEnrolledCourses(string studentId);
}
