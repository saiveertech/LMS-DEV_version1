using System.Data;
using LMS.Application.Features.CourseStudentTracking.DTOs;
using LMS.Application.Features.CourseStudentTracking.Services;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace LMS.Infrastructure.Repositories.CourseStudentTracking;

public class CourseStudentTrackingRepository : ICourseStudentTrackingRepository
{
    private readonly IConfiguration _configuration;

    public CourseStudentTrackingRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private SqlConnection GetConnection()
    {
        return new SqlConnection(
            _configuration.GetConnectionString("DefaultConnection"));
    }

    // ─── Get Course Students ─────────────────────────────────────────────────

    public async Task<List<CourseStudentTrackingResponse>> GetCourseStudents(
        int courseId,
        CourseStudentTrackingFilter filter)
    {
        using var conn = GetConnection();

        using var cmd = new SqlCommand("LMS.SP_GetCourseStudentTracking", conn);

        cmd.CommandType = CommandType.StoredProcedure;

        cmd.Parameters.AddWithValue("@CourseId", courseId);
        cmd.Parameters.AddWithValue(
            "@Status",
            (object?)filter.Status ?? DBNull.Value);
        cmd.Parameters.AddWithValue(
            "@StudentId",
            (object?)filter.StudentId ?? DBNull.Value);
        cmd.Parameters.AddWithValue(
            "@CertificateGenerated",
            (object?)filter.CertificateGenerated ?? DBNull.Value);

        await conn.OpenAsync();

        using var reader = await cmd.ExecuteReaderAsync();

        var students = new List<CourseStudentTrackingResponse>();

        while (await reader.ReadAsync())
        {
            students.Add(MapCourseStudentTrackingResponse(reader));
        }

        return students;
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static CourseStudentTrackingResponse MapCourseStudentTrackingResponse(
        SqlDataReader reader)
    {
        return new CourseStudentTrackingResponse
        {
            StudentId = reader["StudentId"] as string ?? string.Empty,
            StudentName = reader["StudentName"] as string ?? string.Empty,
            Email = reader["Email"] as string ?? string.Empty,
            CourseId = Convert.ToInt32(reader["CourseId"]),
            CourseTitle = reader["CourseTitle"] as string ?? string.Empty,
            EnrollmentDate = Convert.ToDateTime(reader["EnrollmentDate"]),
            RegistrationStatus = reader["RegistrationStatus"] as string ?? string.Empty,
            CourseStatus = reader["CourseStatus"] as string ?? string.Empty,
            CompletionPercentage = Convert.ToDecimal(reader["CompletionPercentage"]),
            CompletedLessons = Convert.ToInt32(reader["CompletedLessons"]),
            TotalLessons = Convert.ToInt32(reader["TotalLessons"]),
            AssessmentScore = reader["AssessmentScore"] == DBNull.Value
                ? null
                : Convert.ToDecimal(reader["AssessmentScore"]),
            PassPercentage = Convert.ToDecimal(reader["PassPercentage"]),
            IsPassed = Convert.ToBoolean(reader["IsPassed"]),
            CertificateGenerated = Convert.ToBoolean(reader["CertificateGenerated"]),
            CertificateId = reader["CertificateId"] as string,
            CertificateIssueDate = reader["CertificateIssueDate"] == DBNull.Value
                ? null
                : Convert.ToDateTime(reader["CertificateIssueDate"]),
            CertificateUrl = reader["CertificateUrl"] as string,
            CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
            UpdatedDate = reader["UpdatedDate"] == DBNull.Value
                ? null
                : Convert.ToDateTime(reader["UpdatedDate"])
        };
    }
}
