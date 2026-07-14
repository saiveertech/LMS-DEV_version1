using System.Data;
using LMS.Application.Features.Certificate.DTOs;
using LMS.Application.Features.Certificate.Services;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace LMS.Infrastructure.Repositories.Certificate;

public class CertificateRepository : ICertificateRepository
{
    private readonly IConfiguration _configuration;

    public CertificateRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private SqlConnection GetConnection()
    {
        return new SqlConnection(
            _configuration.GetConnectionString("DefaultConnection"));
    }

    // ─── Generate Course Certificate ─────────────────────────────────────────

    public async Task<CertificateResponse?> GenerateCourseCertificate(
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
        string createdByRole)
    {
        using var conn = GetConnection();

        using var cmd = new SqlCommand("LMS.SP_GenerateCourseCertificate", conn);

        cmd.CommandType = CommandType.StoredProcedure;

        var certificateIdParam =
            new SqlParameter("@CertificateId", SqlDbType.NVarChar, 50)
            {
                Direction = ParameterDirection.Output
            };

        var credentialIdParam =
            new SqlParameter("@CredentialId", SqlDbType.NVarChar, 50)
            {
                Direction = ParameterDirection.Output
            };

        cmd.Parameters.Add(certificateIdParam);
        cmd.Parameters.Add(credentialIdParam);
        cmd.Parameters.AddWithValue("@StudentId", studentId);
        cmd.Parameters.AddWithValue("@StudentName", studentName);
        cmd.Parameters.AddWithValue("@StudentEmail", studentEmail);
        cmd.Parameters.AddWithValue("@CourseId", courseId);
        cmd.Parameters.AddWithValue("@CourseName", courseName);
        cmd.Parameters.AddWithValue("@CompletionPercentage", completionPercentage);
        cmd.Parameters.AddWithValue("@CompletionDate", completionDate);
        cmd.Parameters.AddWithValue("@CertificateUrl", certificateUrl);
        cmd.Parameters.AddWithValue("@CreatedById", createdById);
        cmd.Parameters.AddWithValue("@CreatedByName", createdByName);
        cmd.Parameters.AddWithValue("@CreatedByRole", createdByRole);

        await conn.OpenAsync();

        using var reader = await cmd.ExecuteReaderAsync();

        return await reader.ReadAsync() ? MapCertificateResponse(reader) : null;
    }

    // ─── Generate Assignment Certificate ─────────────────────────────────────

    public async Task<CertificateResponse?> GenerateAssignmentCertificate(
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
        string createdByRole)
    {
        using var conn = GetConnection();

        using var cmd = new SqlCommand("LMS.SP_GenerateAssignmentCertificate", conn);

        cmd.CommandType = CommandType.StoredProcedure;

        var certificateIdParam =
            new SqlParameter("@CertificateId", SqlDbType.NVarChar, 50)
            {
                Direction = ParameterDirection.Output
            };

        var credentialIdParam =
            new SqlParameter("@CredentialId", SqlDbType.NVarChar, 50)
            {
                Direction = ParameterDirection.Output
            };

        cmd.Parameters.Add(certificateIdParam);
        cmd.Parameters.Add(credentialIdParam);
        cmd.Parameters.AddWithValue("@StudentId", studentId);
        cmd.Parameters.AddWithValue("@StudentName", studentName);
        cmd.Parameters.AddWithValue("@StudentEmail", studentEmail);
        cmd.Parameters.AddWithValue("@AssignmentId", assignmentId);
        cmd.Parameters.AddWithValue("@AssignmentTitle", assignmentTitle);
        cmd.Parameters.AddWithValue("@AssessmentScore", assessmentScore);
        cmd.Parameters.AddWithValue("@PassPercentage", passPercentage);
        cmd.Parameters.AddWithValue("@CompletionDate", completionDate);
        cmd.Parameters.AddWithValue("@CertificateUrl", certificateUrl);
        cmd.Parameters.AddWithValue("@CreatedById", createdById);
        cmd.Parameters.AddWithValue("@CreatedByName", createdByName);
        cmd.Parameters.AddWithValue("@CreatedByRole", createdByRole);

        await conn.OpenAsync();

        using var reader = await cmd.ExecuteReaderAsync();

        return await reader.ReadAsync() ? MapCertificateResponse(reader) : null;
    }

    // ─── Get Student Certificates ────────────────────────────────────────────

    public async Task<List<CertificateResponse>> GetStudentCertificates(string studentId)
    {
        using var conn = GetConnection();

        using var cmd = new SqlCommand("LMS.SP_GetStudentCertificates", conn);

        cmd.CommandType = CommandType.StoredProcedure;

        cmd.Parameters.AddWithValue("@StudentId", studentId);

        await conn.OpenAsync();

        using var reader = await cmd.ExecuteReaderAsync();

        var certificates = new List<CertificateResponse>();

        while (await reader.ReadAsync())
        {
            certificates.Add(MapCertificateResponse(reader));
        }

        return certificates;
    }

    // ─── Get Certificate By Id ───────────────────────────────────────────────

    public async Task<CertificateResponse?> GetCertificateById(string certificateId)
    {
        using var conn = GetConnection();

        using var cmd = new SqlCommand("LMS.SP_GetCertificateById", conn);

        cmd.CommandType = CommandType.StoredProcedure;

        cmd.Parameters.AddWithValue("@CertificateId", certificateId);

        await conn.OpenAsync();

        using var reader = await cmd.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return MapCertificateResponse(reader);
        }

        return null;
    }

    // ─── Verify Certificate ──────────────────────────────────────────────────

    public async Task<VerifyCertificateResponse?> VerifyCertificate(string certificateId)
    {
        using var conn = GetConnection();

        using var cmd = new SqlCommand("LMS.SP_VerifyCertificate", conn);

        cmd.CommandType = CommandType.StoredProcedure;

        cmd.Parameters.AddWithValue("@CertificateId", certificateId);

        await conn.OpenAsync();

        using var reader = await cmd.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new VerifyCertificateResponse
            {
                CertificateId = reader["CertificateId"] as string ?? string.Empty,
                CredentialId = reader["CredentialId"] as string ?? string.Empty,
                CertificateType = reader["CertificateType"] as string ?? string.Empty,
                StudentName = reader["StudentName"] as string ?? string.Empty,
                CourseName = reader["CourseName"] as string,
                CompletionPercentage = reader["CompletionPercentage"] == DBNull.Value
                    ? null
                    : Convert.ToDecimal(reader["CompletionPercentage"]),
                AssessmentScore = reader["AssessmentScore"] == DBNull.Value
                    ? null
                    : Convert.ToDecimal(reader["AssessmentScore"]),
                PassPercentage = reader["PassPercentage"] == DBNull.Value
                    ? null
                    : Convert.ToDecimal(reader["PassPercentage"]),
                CompletionDate = Convert.ToDateTime(reader["CompletionDate"]),
                IssuedDate = Convert.ToDateTime(reader["IssuedDate"]),
                IsValid = Convert.ToBoolean(reader["IsValid"])
            };
        }

        return null;
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static CertificateResponse MapCertificateResponse(SqlDataReader reader)
    {
        return new CertificateResponse
        {
            Id = Convert.ToInt32(reader["Id"]),
            CertificateId = reader["CertificateId"] as string ?? string.Empty,
            CredentialId = reader["CredentialId"] as string ?? string.Empty,
            CertificateType = reader["CertificateType"] as string ?? string.Empty,
            StudentId = reader["StudentId"] as string ?? string.Empty,
            StudentName = reader["StudentName"] as string ?? string.Empty,
            StudentEmail = reader["StudentEmail"] as string ?? string.Empty,
            CourseId = reader["CourseId"] == DBNull.Value
                ? null
                : Convert.ToInt32(reader["CourseId"]),
            CourseName = reader["CourseName"] as string,
            AssignmentId = reader["AssignmentId"] == DBNull.Value
                ? null
                : Convert.ToInt32(reader["AssignmentId"]),
            CompletionPercentage = reader["CompletionPercentage"] == DBNull.Value
                ? null
                : Convert.ToDecimal(reader["CompletionPercentage"]),
            AssessmentScore = reader["AssessmentScore"] == DBNull.Value
                ? null
                : Convert.ToDecimal(reader["AssessmentScore"]),
            PassPercentage = reader["PassPercentage"] == DBNull.Value
                ? null
                : Convert.ToDecimal(reader["PassPercentage"]),
            CompletionDate = Convert.ToDateTime(reader["CompletionDate"]),
            IssuedDate = Convert.ToDateTime(reader["IssuedDate"]),
            CertificateUrl = reader["CertificateUrl"] as string ?? string.Empty,
            IsValid = Convert.ToBoolean(reader["IsValid"]),
            CreatedById = reader["CreatedById"] as string ?? string.Empty,
            CreatedByName = reader["CreatedByName"] as string ?? string.Empty,
            CreatedByRole = reader["CreatedByRole"] as string ?? string.Empty,
            CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
            UpdatedDate = reader["UpdatedDate"] == DBNull.Value
                ? null
                : Convert.ToDateTime(reader["UpdatedDate"])
        };
    }
}
