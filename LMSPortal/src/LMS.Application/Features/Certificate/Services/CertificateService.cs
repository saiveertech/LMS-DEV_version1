using LMS.Application.Common;
using LMS.Application.Features.Assignment.Services;
using LMS.Application.Features.Auth.Services.Student;
using LMS.Application.Features.Certificate.DTOs;
using LMS.Application.Features.Course.Services;

namespace LMS.Application.Features.Certificate.Services;

public class CertificateService : ICertificateService
{
    private readonly ICertificateRepository _repo;
    private readonly ICourseRepository _courseRepo;
    private readonly IAssignmentRepository _assignmentRepo;
    private readonly IStudentRepository _studentRepo;
    private readonly ICertificateTemplateRenderer _templateRenderer;
    private readonly IBlobStorageService _blobService;

    private const string CertificatesContainer = "certificates";

    public CertificateService(
        ICertificateRepository repo,
        ICourseRepository courseRepo,
        IAssignmentRepository assignmentRepo,
        IStudentRepository studentRepo,
        ICertificateTemplateRenderer templateRenderer,
        IBlobStorageService blobService)
    {
        _repo = repo;
        _courseRepo = courseRepo;
        _assignmentRepo = assignmentRepo;
        _studentRepo = studentRepo;
        _templateRenderer = templateRenderer;
        _blobService = blobService;
    }

    private async Task<(string Name, string Email)?> GetStudentNameAndEmail(string studentId)
    {
        var studentObj = await _studentRepo.GetStudentById(studentId);

        if (studentObj == null)
            return null;

        var studentType = studentObj.GetType();

        var firstName = studentType.GetProperty("FirstName")?.GetValue(studentObj) as string ?? string.Empty;
        var lastName = studentType.GetProperty("LastName")?.GetValue(studentObj) as string ?? string.Empty;
        var email = studentType.GetProperty("Email")?.GetValue(studentObj) as string ?? string.Empty;

        return ($"{firstName} {lastName}".Trim(), email);
    }

    //=========================================
    // Generate Course Certificate (auto-issued)
    //=========================================

    public async Task<ServiceResponse> GenerateCourseCertificateAsync(
        string studentId,
        int courseId,
        decimal completionPercentage,
        DateTime completionDate,
        string createdById,
        string createdByName,
        string createdByRole)
    {
        try
        {
            var courseObj = await _courseRepo.GetCourseById(courseId);

            if (courseObj == null)
            {
                return new ServiceResponse { Success = false, Message = "Course not found." };
            }

            var courseName = courseObj.GetType().GetProperty("Title")?.GetValue(courseObj) as string
                ?? string.Empty;

            var student = await GetStudentNameAndEmail(studentId);

            if (student == null)
            {
                return new ServiceResponse { Success = false, Message = "Student not found." };
            }

            var tempCertId = $"CERT-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
            var tempCredId = $"CRED-CO-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";

            var pngBytes = await _templateRenderer.RenderPngAsync(
                student.Value.Name,
                courseName,
                completionDate,
                tempCredId,
                tempCertId);

            var pdfBytes = _templateRenderer.WrapPngInPdf(pngBytes);

            var blobName = $"{studentId}/{tempCertId}.pdf";

            string certificateUrl;

            using (var stream = new MemoryStream(pdfBytes))
            {
                certificateUrl = await _blobService.UploadStreamAsync(
                    stream,
                    CertificatesContainer,
                    blobName,
                    "application/pdf");
            }

            var certificate = await _repo.GenerateCourseCertificate(
                studentId,
                student.Value.Name,
                student.Value.Email,
                courseId,
                courseName,
                completionPercentage,
                completionDate,
                certificateUrl,
                createdById,
                createdByName,
                createdByRole);

            if (certificate == null)
            {
                return new ServiceResponse { Success = false, Message = "Failed to generate course certificate." };
            }

            return new ServiceResponse
            {
                Success = true,
                Message = "Course certificate generated successfully.",
                Data = certificate
            };
        }
        catch (Exception ex)
        {
            return new ServiceResponse { Success = false, Message = ex.Message };
        }
    }

    //=========================================
    // Generate Assignment Certificate (auto-issued)
    //=========================================

    public async Task<ServiceResponse> GenerateAssignmentCertificateAsync(
        string studentId,
        int assignmentId,
        decimal assessmentScore,
        DateTime completionDate,
        string createdById,
        string createdByName,
        string createdByRole)
    {
        try
        {
            var assignmentObj = await _assignmentRepo.GetAssignments(assignmentId);

            if (assignmentObj == null)
            {
                return new ServiceResponse { Success = false, Message = "Assignment not found." };
            }

            var assignmentType = assignmentObj.GetType();

            var assignmentTitle = assignmentType.GetProperty("Title")?.GetValue(assignmentObj) as string
                ?? string.Empty;

            var passPercentage = Convert.ToDecimal(
                assignmentType.GetProperty("PassPercentage")?.GetValue(assignmentObj) ?? 0m);

            var student = await GetStudentNameAndEmail(studentId);

            if (student == null)
            {
                return new ServiceResponse { Success = false, Message = "Student not found." };
            }

            var tempCertId = $"CERT-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
            var tempCredId = $"CRED-AS-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";

            var pngBytes = await _templateRenderer.RenderPngAsync(
                student.Value.Name,
                assignmentTitle,
                completionDate,
                tempCredId,
                tempCertId);

            var pdfBytes = _templateRenderer.WrapPngInPdf(pngBytes);

            var blobName = $"{studentId}/{tempCertId}.pdf";

            string certificateUrl;

            using (var stream = new MemoryStream(pdfBytes))
            {
                certificateUrl = await _blobService.UploadStreamAsync(
                    stream,
                    CertificatesContainer,
                    blobName,
                    "application/pdf");
            }

            var certificate = await _repo.GenerateAssignmentCertificate(
                studentId,
                student.Value.Name,
                student.Value.Email,
                assignmentId,
                assignmentTitle,
                assessmentScore,
                passPercentage,
                completionDate,
                certificateUrl,
                createdById,
                createdByName,
                createdByRole);

            if (certificate == null)
            {
                return new ServiceResponse { Success = false, Message = "Failed to generate assignment certificate." };
            }

            return new ServiceResponse
            {
                Success = true,
                Message = "Assignment certificate generated successfully.",
                Data = certificate
            };
        }
        catch (Exception ex)
        {
            return new ServiceResponse { Success = false, Message = ex.Message };
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
