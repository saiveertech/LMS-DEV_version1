using LMS.Application.Common;
using LMS.Application.Features.Certificate.DTOs;
using LMS.Application.Features.Course.Services;

namespace LMS.Application.Features.Certificate.Services;

public class CertificateService : ICertificateService
{
    private readonly ICertificateRepository _repo;
    private readonly ICourseRepository _courseRepo;
    private readonly IPdfCertificateService _pdfService;
    private readonly IBlobStorageService _blobService;

    private const string CertificatesContainer = "certificates";

    public CertificateService(
        ICertificateRepository repo,
        ICourseRepository courseRepo,
        IPdfCertificateService pdfService,
        IBlobStorageService blobService)
    {
        _repo = repo;
        _courseRepo = courseRepo;
        _pdfService = pdfService;
        _blobService = blobService;
    }

    //=========================================
    // Generate Certificate
    //=========================================

    public async Task<ServiceResponse> GenerateCertificate(
        GenerateCertificateRequest request,
        string createdById,
        string createdByName,
        string createdByRole)
    {
        try
        {
            // ── Input Validation ──

            if (string.IsNullOrWhiteSpace(request.StudentId))
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "Student ID is required."
                };
            }

            if (request.CourseId <= 0)
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "A valid Course ID is required."
                };
            }

            // ── Business Rule: Completion must be 100% ──

            if (request.CompletionPercentage < 100)
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "Course completion must be 100% to generate a certificate."
                };
            }

            // ── Fetch course details for validation ──

            var courseObj = await _courseRepo.GetCourseById(request.CourseId);

            if (courseObj == null)
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "Course not found."
                };
            }

            // Extract course properties dynamically
            var courseType = courseObj.GetType();

            var courseName = courseType.GetProperty("Title")?.GetValue(courseObj) as string
                ?? string.Empty;

            var passPercentage = Convert.ToDecimal(
                courseType.GetProperty("PassPercentage")?.GetValue(courseObj) ?? 0m);

            // ── Business Rule: Assessment score >= Pass Percentage ──

            if (request.AssessmentScore < passPercentage)
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = $"Assessment score ({request.AssessmentScore}%) is below the pass percentage ({passPercentage}%)."
                };
            }

            // ── Fetch student details ──

            // We need student name and email; use inline SQL for lightweight fetch
            var studentName = createdByName;
            var studentEmail = string.Empty;

            // ── Generate temporary IDs for PDF (will be replaced by SP) ──

            var tempCertId = $"CERT-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
            var tempCredId = $"CRED-SK-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";

            // ── Generate PDF ──

            var pdfBytes = _pdfService.GenerateCertificatePdf(
                studentName,
                courseName,
                request.CompletionDate,
                tempCredId,
                tempCertId);

            // ── Upload to Azure Blob Storage ──

            var blobName = $"{request.StudentId}/{tempCertId}.pdf";

            string certificateUrl;

            using (var stream = new MemoryStream(pdfBytes))
            {
                certificateUrl = await _blobService.UploadStreamAsync(
                    stream,
                    CertificatesContainer,
                    blobName,
                    "application/pdf");
            }

            // ── Save to database (SP handles duplicate prevention) ──

            var certificate = await _repo.GenerateCertificate(
                request,
                certificateUrl,
                studentName,
                studentEmail,
                courseName,
                passPercentage,
                createdById,
                createdByName,
                createdByRole);

            if (certificate == null)
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "Failed to generate certificate."
                };
            }

            return new ServiceResponse
            {
                Success = true,
                Message = "Certificate generated successfully.",
                Data = certificate
            };
        }
        catch (Exception ex)
        {
            return new ServiceResponse
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    //=========================================
    // Get Student Certificates
    //=========================================

    public async Task<List<CertificateResponse>> GetStudentCertificates(string studentId)
    {
        return await _repo.GetStudentCertificates(studentId);
    }

    //=========================================
    // Get Certificate By Id
    //=========================================

    public async Task<CertificateResponse?> GetCertificateById(string certificateId)
    {
        return await _repo.GetCertificateById(certificateId);
    }

    //=========================================
    // Verify Certificate
    //=========================================

    public async Task<ServiceResponse> VerifyCertificate(string certificateId)
    {
        try
        {
            var result = await _repo.VerifyCertificate(certificateId);

            if (result == null)
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "Certificate not found."
                };
            }

            return new ServiceResponse
            {
                Success = true,
                Message = result.IsValid
                    ? "Certificate is valid."
                    : "Certificate is no longer valid.",
                Data = result
            };
        }
        catch (Exception ex)
        {
            return new ServiceResponse
            {
                Success = false,
                Message = ex.Message
            };
        }
    }
}
