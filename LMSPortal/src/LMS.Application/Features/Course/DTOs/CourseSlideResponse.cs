namespace LMS.Application.Features.Course.DTOs;

public class CourseSlideResponse
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string MediaType { get; set; } = string.Empty;

    public string MediaUrl { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; }
}
