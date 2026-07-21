using LMS.Application.Common;
using LMS.Application.Features.Auth.DTOs;
using LMS.Application.Features.CourseStudentTracking.DTOs;

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

    Task<ServiceResponse> SubmitAssignmentAttempt(SubmitAssignmentAttemptRequest request);

    // Trainer/admin-initiated: creates the same enrollment as self-registration
    // (EnrollCourse/EnrollAssignment, unchanged) plus an audit record of who assigned it.
    Task<ServiceResponse> AssignCourseToStudent(
        string studentId, int courseId,
        string assignedById, string assignedByName, string assignedByRole);

    Task<ServiceResponse> AssignAssignmentToStudent(
        string studentId, int assignmentId,
        string assignedById, string assignedByName, string assignedByRole);

    // Trainer/admin-assigned courses/assignments for this student — the
    // audit trail from AssignCourseToStudent/AssignAssignmentToStudent,
    // read back. Self-registrations are excluded.
    Task<object?> GetAssignedCourses(string studentId);

    Task<object?> GetAssignedAssignments(string studentId);

    Task<object?> GetRecommendedCourses(string studentId, int topN);

    Task<object?> GetRecommendedAssignments(string studentId, int topN);

    Task<ServiceResponse> CompleteCourseSlide(string studentId, int slideId);

    Task<List<CourseSlideProgressResponse>> GetCourseResume(string studentId, int courseId);

    Task<ServiceResponse> SubmitAssignmentAnswers(SubmitAssignmentAnswersRequest request);
}