using LMS.Application.Common;
using LMS.Application.Features.Auth.DTOs;

namespace LMS.Application.Features.Auth.Services.Student;

public interface IStudentService
{
    Task<ServiceResponse> RegisterStudent(RegisterStudentRequest request);

    Task<object?> GetStudentById(string? studentId = null);

    Task<ServiceResponse> UpdateStudent(
        string studentId,
        UpdateStudentRequest request);

    Task<ServiceResponse> EnrollCourse(EnrollCourseRequest request);

    Task<object?> GetEnrolledCourses(string studentId);

    Task<ServiceResponse> EnrollAssignment(EnrollAssignmentRequest request);

    Task<object?> GetEnrolledAssignments(string studentId);
}