using Microsoft.AspNetCore.Http;

namespace LMS.Application.Features.Course.DTOs;

public enum SlideMediaType
{
    Video,
    Url
}

public class CourseSlideRequest
{
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public SlideMediaType MediaType { get; set; }

    // Required when MediaType is Video.
    public IFormFile? VideoFile { get; set; }

    // Required when MediaType is Url (e.g. a YouTube link).
    public string? MediaUrl { get; set; }

    public int SortOrder { get; set; }
}

// Resolved slide data (video already uploaded to blob storage, or the
// external URL as-is) ready to persist — no IFormFile at this point.
public class CourseSlideInput
{
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string MediaType { get; set; } = string.Empty;

    public string MediaUrl { get; set; } = string.Empty;

    public int SortOrder { get; set; }
}
