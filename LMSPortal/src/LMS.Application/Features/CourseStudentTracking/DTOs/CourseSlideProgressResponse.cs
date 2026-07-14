namespace LMS.Application.Features.CourseStudentTracking.DTOs;

public class CourseSlideProgressResponse
{
    public int SlideId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string MediaType { get; set; } = string.Empty;

    public string MediaUrl { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsCompleted { get; set; }

    public DateTime? CompletedAt { get; set; }
}
