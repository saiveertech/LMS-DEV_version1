using LMS.Application.Features.Auth.DTOs;
using LMS.Application.Features.CourseStudentTracking.DTOs;

namespace LMS.Application.Features.Auth.Services.Student;

public interface IStudentRepository
{
    Task<object> RegisterStudent(RegisterStudentRequest request);
    Task<object?> GetStudentById(string? studentId = null);
    Task<bool> UpdateStudent(string studentId, UpdateStudentRequest request);

    Task<object> EnrollCourse(EnrollCourseRequest request);
    Task<object?> GetEnrolledCourses(string studentId);

    Task<object> EnrollAssignment(EnrollAssignmentRequest request);
    Task<object?> GetEnrolledAssignments(string studentId);

    // Throws if the student isn't enrolled in this assignment.
    Task<object> SubmitAssignmentAttempt(SubmitAssignmentAttemptRequest request);

    // Audit-only inserts — recorded alongside EnrollCourse/EnrollAssignment
    // when a trainer/admin (not the student) initiated the enrollment.
    Task RecordCourseAssignment(
        string studentId, int courseId,
        string assignedById, string assignedByName, string assignedByRole);

    Task RecordAssignmentAllocation(
        string studentId, int assignmentId,
        string assignedById, string assignedByName, string assignedByRole);

    // Reads back the audit trail above — only trainer/admin-assigned courses
    // and assignments for this student, self-registrations excluded.
    Task<object?> GetAssignedCourses(string studentId);

    Task<object?> GetAssignedAssignments(string studentId);

    // Throws if the slide or the student's course enrollment isn't found.
    Task<CourseStudentTrackingResponse?> CompleteCourseSlide(string studentId, int slideId);

    Task<List<CourseSlideProgressResponse>> GetCourseResume(string studentId, int courseId);

    // Records one attempt's already-evaluated answers. Returns the
    // self-determined attempt number.
    Task<int> SaveStudentAnswers(
        string studentId, int assignmentId,
        List<(int QuestionId, string SelectedOption, bool IsCorrect)> answers);
}
