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

        var assignments = new List<AssignmentResponse>();

        while (await reader.ReadAsync())
        {
            assignments.Add(new AssignmentResponse
            {
                AssignmentId = Convert.ToInt32(reader["AssignmentId"]),
                Title = reader["Title"] as string ?? string.Empty,
                Description = reader["Description"] as string,
                IntroVideoUrl = reader["IntroVideoUrl"] as string,
                QuestionsCsvUrl = reader["QuestionsCsvUrl"] as string,
                CompletionTimeSeconds = Convert.ToInt32(reader["CompletionTimeSeconds"]),
                PassPercentage = Convert.ToDecimal(reader["PassPercentage"]),
                WwEnvClientId = reader["WwEnvClientId"] as string,
                AssessmentIconUrl = reader["AssessmentIconUrl"] as string,
                Tags = reader["Tags"] as string,
                CreatedById = reader["CreatedById"] as string ?? string.Empty,
                CreatedByName = reader["CreatedByName"] as string ?? string.Empty,
                CreatedByRole = reader["CreatedByRole"] as string ?? string.Empty,
                EditedById = reader["EditedById"] as string,
                EditedByName = reader["EditedByName"] as string,
                EditedByRole = reader["EditedByRole"] as string,
                DeletedById = reader["DeletedById"] as string,
                DeletedByName = reader["DeletedByName"] as string,
                DeletedByRole = reader["DeletedByRole"] as string,
                DeletedAt = reader["DeletedAt"] as DateTime?,
                CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                UpdatedAt = reader["UpdatedAt"] as DateTime?
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
        string? assessmentIconUrl,
        string editedById,
        string editedByName,
        string editedByRole)
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

        cmd.Parameters.AddWithValue("@EditedById", editedById);
        cmd.Parameters.AddWithValue("@EditedByName", editedByName);
        cmd.Parameters.AddWithValue("@EditedByRole", editedByRole);

        await conn.OpenAsync();

        var result = await cmd.ExecuteScalarAsync();

        int rows = Convert.ToInt32(result);

        return rows > 0;
    }

    public async Task<bool> DeleteAssignment(
        int assignmentId,
        string deletedById,
        string deletedByName,
        string deletedByRole)
    {
        using var conn = GetConnection();

        using var cmd = new SqlCommand("LMS.SP_DeleteAssignment", conn);

        cmd.CommandType = CommandType.StoredProcedure;

        cmd.Parameters.AddWithValue("@AssignmentId", assignmentId);
        cmd.Parameters.AddWithValue("@DeletedById", deletedById);
        cmd.Parameters.AddWithValue("@DeletedByName", deletedByName);
        cmd.Parameters.AddWithValue("@DeletedByRole", deletedByRole);

        await conn.OpenAsync();

        var result = await cmd.ExecuteScalarAsync();

        int rows = Convert.ToInt32(result);

        return rows > 0;
    }
}
