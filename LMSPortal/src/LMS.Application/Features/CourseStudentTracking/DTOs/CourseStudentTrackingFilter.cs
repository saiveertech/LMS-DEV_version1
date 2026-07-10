namespace LMS.Application.Features.CourseStudentTracking.DTOs;

public class CourseStudentTrackingFilter
{
    public string? Status { get; set; }

    public string? StudentId { get; set; }

    public bool? CertificateGenerated { get; set; }
}
