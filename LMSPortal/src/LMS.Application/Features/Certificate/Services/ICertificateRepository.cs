using LMS.Application.Features.Certificate.DTOs;

namespace LMS.Application.Features.Certificate.Services;

public interface ICertificateRepository
{
    // Throws if a course certificate already exists for this student/course.
    Task<CertificateResponse?> GenerateCourseCertificate(
        string studentId,
        string studentName,
        string studentEmail,
        int courseId,
        string courseName,
        decimal completionPercentage,
        DateTime completionDate,
        string certificateUrl,
        string createdById,
        string createdByName,
        string createdByRole);

    // Throws if an assignment certificate already exists for this student/assignment.
    Task<CertificateResponse?> GenerateAssignmentCertificate(
        string studentId,
        string studentName,
        string studentEmail,
        int assignmentId,
        string assignmentTitle,
        decimal assessmentScore,
        decimal passPercentage,
        DateTime completionDate,
        string certificateUrl,
        string createdById,
        string createdByName,
        string createdByRole);

    Task<List<CertificateResponse>> GetStudentCertificates(string studentId);

    Task<CertificateResponse?> GetCertificateById(string certificateId);

    Task<VerifyCertificateResponse?> VerifyCertificate(string certificateId);
}
