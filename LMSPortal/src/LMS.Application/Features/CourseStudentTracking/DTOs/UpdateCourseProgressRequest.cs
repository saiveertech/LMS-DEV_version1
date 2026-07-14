namespace LMS.Application.Features.CourseStudentTracking.DTOs;

public class UpdateCourseProgressRequest
{
    public int CompletedLessons { get; set; }

    // Optional — if omitted, keeps the enrollment's existing TotalLessons.
    public int? TotalLessons { get; set; }
}
