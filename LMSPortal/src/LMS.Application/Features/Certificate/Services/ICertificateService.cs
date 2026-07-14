using LMS.Application.Common;
using LMS.Application.Features.Certificate.DTOs;

namespace LMS.Application.Features.Certificate.Services;

public interface ICertificateService
{
    // Auto-issued: called by the system the moment a course enrollment
    // reaches 100% completion. Not exposed as a manual endpoint.
    Task<ServiceResponse> GenerateCourseCertificateAsync(
        string studentId,
        int courseId,
        decimal completionPercentage,
        DateTime completionDate,
        string createdById,
        string createdByName,
        string createdByRole);

    // Auto-issued: called by the system the moment a student passes an
    // assignment. Not exposed as a manual endpoint.
    Task<ServiceResponse> GenerateAssignmentCertificateAsync(
        string studentId,
        int assignmentId,
        decimal assessmentScore,
        DateTime completionDate,
        string createdById,
        string createdByName,
        string createdByRole);

    Task<List<CertificateResponse>> GetStudentCertificates(string studentId);

    Task<CertificateResponse?> GetCertificateById(string certificateId);

    Task<ServiceResponse> VerifyCertificate(string certificateId);
}
