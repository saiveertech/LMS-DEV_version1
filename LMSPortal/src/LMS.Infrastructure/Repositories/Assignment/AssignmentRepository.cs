using System.Data;
using LMS.Application.Features.Assignment.DTOs;
using LMS.Application.Features.Assignment.Services;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace LMS.Infrastructure.Repositories.Assignment;

public class AssignmentRepository : IAssignmentRepository
{
    private readonly IConfiguration _configuration;

    public AssignmentRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private SqlConnection GetConnection()
    {
        return new SqlConnection(
            _configuration.GetConnectionString("DefaultConnection"));
    }

    public async Task<object> CreateAssignment(
        CreateAssignmentRequest request,
        string introVideoUrl,
        string questionsCsvUrl,
        string assessmentIconUrl,
        string createdById,
        string createdByName,
        string createdByRole)
    {
        using var conn = GetConnection();

        using var cmd = new SqlCommand("LMS.SP_CreateAssignment", conn);

        cmd.CommandType = CommandType.StoredProcedure;

        cmd.Parameters.AddWithValue("@Title", request.Title);

        cmd.Parameters.AddWithValue(
            "@Description",
            string.IsNullOrWhiteSpace(request.Description)
                ? DBNull.Value
                : request.Description);

        cmd.Parameters.AddWithValue(
            "@IntroVideoUrl",
            string.IsNullOrWhiteSpace(introVideoUrl)
                ? DBNull.Value
                : introVideoUrl);

        cmd.Parameters.AddWithValue(
            "@QuestionsCsvUrl",
            string.IsNullOrWhiteSpace(questionsCsvUrl)
                ? DBNull.Value
                : questionsCsvUrl);

        cmd.Parameters.AddWithValue("@CompletionTimeSeconds", request.CompletionTimeSeconds);

        cmd.Parameters.AddWithValue("@PassPercentage", request.PassPercentage);

        cmd.Parameters.AddWithValue(
            "@WwEnvClientId",
            string.IsNullOrWhiteSpace(request.WwEnvClientId)
                ? DBNull.Value
                : request.WwEnvClientId);

        cmd.Parameters.AddWithValue(
            "@AssessmentIconUrl",
            string.IsNullOrWhiteSpace(assessmentIconUrl)
                ? DBNull.Value
                : assessmentIconUrl);

        cmd.Parameters.AddWithValue(
            "@Tags",
            string.IsNullOrWhiteSpace(request.Tags)
                ? DBNull.Value
                : request.Tags);

        cmd.Parameters.AddWithValue("@CreatedById", createdById);
        cmd.Parameters.AddWithValue("@CreatedByName", createdByName);
        cmd.Parameters.AddWithValue("@CreatedByRole", createdByRole);

        await conn.OpenAsync();

        var newId = Convert.ToInt32(await cmd.ExecuteScalarAsync());

        return await GetAssignments(newId) ?? new { AssignmentId = newId };
    }

    public async Task<object?> GetAssignments(int? assignmentId = null)
    {
        using var conn = GetConnection();

        using var cmd = new SqlCommand("LMS.SP_GetAssignments", conn);

        cmd.CommandType = CommandType.StoredProcedure;

        cmd.Parameters.AddWithValue("@AssignmentId", (object?)assignmentId ?? DBNull.Value);

        await conn.OpenAsync();

        using var reader = await cmd.ExecuteReaderAsync();

        var assignments = new List<object>();

        while (await reader.ReadAsync())
        {
            assignments.Add(new
            {
                AssignmentId = reader["AssignmentId"],
                Title = reader["Title"],
                Description = reader["Description"],
                IntroVideoUrl = reader["IntroVideoUrl"],
                QuestionsCsvUrl = reader["QuestionsCsvUrl"],
                CompletionTimeSeconds = reader["CompletionTimeSeconds"],
                PassPercentage = reader["PassPercentage"],
                WwEnvClientId = reader["WwEnvClientId"],
                AssessmentIconUrl = reader["AssessmentIconUrl"],
                Tags = reader["Tags"],
                CreatedById = reader["CreatedById"],
                CreatedByName = reader["CreatedByName"],
                CreatedByRole = reader["CreatedByRole"],
                CreatedAt = reader["CreatedAt"],
                UpdatedAt = reader["UpdatedAt"]
            });
        }

        if (assignmentId.HasValue)
            return assignments.Count > 0 ? assignments[0] : null;

        return assignments;
    }

    public async Task<bool> UpdateAssignment(
        int assignmentId,
        UpdateAssignmentRequest request,
        string? introVideoUrl,
        string? questionsCsvUrl,
        string? assessmentIconUrl)
    {
        using var conn = GetConnection();

        using var cmd = new SqlCommand("LMS.SP_UpdateAssignment", conn);

        cmd.CommandType = CommandType.StoredProcedure;

        cmd.Parameters.AddWithValue("@AssignmentId", assignmentId);

        cmd.Parameters.AddWithValue(
            "@Title",
            string.IsNullOrWhiteSpace(request.Title)
                ? DBNull.Value
                : request.Title);

        cmd.Parameters.AddWithValue(
            "@Description",
            string.IsNullOrWhiteSpace(request.Description)
                ? DBNull.Value
                : request.Description);

        cmd.Parameters.AddWithValue(
            "@IntroVideoUrl",
            string.IsNullOrWhiteSpace(introVideoUrl)
                ? DBNull.Value
                : introVideoUrl);

        cmd.Parameters.AddWithValue(
            "@QuestionsCsvUrl",
            string.IsNullOrWhiteSpace(questionsCsvUrl)
                ? DBNull.Value
                : questionsCsvUrl);

        cmd.Parameters.AddWithValue(
            "@CompletionTimeSeconds",
            request.CompletionTimeSeconds.HasValue
                ? request.CompletionTimeSeconds.Value
                : DBNull.Value);

        cmd.Parameters.AddWithValue(
            "@PassPercentage",
            request.PassPercentage.HasValue
                ? request.PassPercentage.Value
                : DBNull.Value);

        cmd.Parameters.AddWithValue(
            "@WwEnvClientId",
            string.IsNullOrWhiteSpace(request.WwEnvClientId)
                ? DBNull.Value
                : request.WwEnvClientId);

        cmd.Parameters.AddWithValue(
            "@AssessmentIconUrl",
            string.IsNullOrWhiteSpace(assessmentIconUrl)
                ? DBNull.Value
                : assessmentIconUrl);

        cmd.Parameters.AddWithValue(
            "@Tags",
            string.IsNullOrWhiteSpace(request.Tags)
                ? DBNull.Value
                : request.Tags);

        await conn.OpenAsync();

        var result = await cmd.ExecuteScalarAsync();

        int rows = Convert.ToInt32(result);

        return rows > 0;
    }

    public async Task<bool> DeleteAssignment(int assignmentId)
    {
        using var conn = GetConnection();

        using var cmd = new SqlCommand("LMS.SP_DeleteAssignment", conn);

        cmd.CommandType = CommandType.StoredProcedure;

        cmd.Parameters.AddWithValue("@AssignmentId", assignmentId);

        await conn.OpenAsync();

        var result = await cmd.ExecuteScalarAsync();

        int rows = Convert.ToInt32(result);

        return rows > 0;
    }
}
