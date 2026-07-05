using System.Data;
using LMS.Application.Features.Course.DTOs;
using LMS.Application.Features.Course.Services;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace LMS.Infrastructure.Repositories.Course;

public class CourseRepository : ICourseRepository
{
    private readonly IConfiguration _configuration;

    public CourseRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private SqlConnection GetConnection()
    {
        return new SqlConnection(
            _configuration.GetConnectionString("DefaultConnection"));
    }

    public async Task<object> RegisterCourse(
        CreateCourseRequest request,
        string introVideoUrl,
        string courseIconUrl,
        string createdById,
        string createdByName,
        string createdByRole)
    {
        using var conn = GetConnection();

        using var cmd = new SqlCommand("LMS.SP_RegisterCourse", conn);

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
            "@SlidesJson",
            string.IsNullOrWhiteSpace(request.SlidesJson)
                ? DBNull.Value
                : request.SlidesJson);

        cmd.Parameters.AddWithValue("@CompletionTimeSeconds", request.CompletionTimeSeconds);

        cmd.Parameters.AddWithValue("@PassPercentage", request.PassPercentage);

        cmd.Parameters.AddWithValue(
            "@WwEnvClientId",
            string.IsNullOrWhiteSpace(request.WwEnvClientId)
                ? DBNull.Value
                : request.WwEnvClientId);

        cmd.Parameters.AddWithValue(
            "@CourseIconUrl",
            string.IsNullOrWhiteSpace(courseIconUrl)
                ? DBNull.Value
                : courseIconUrl);

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

        return await GetCourseById(newId) ?? new { Id = newId };
    }

    public async Task<object?> GetCourseById(int? courseId = null)
    {
        using var conn = GetConnection();

        using var cmd = new SqlCommand("LMS.SP_GetCourseById", conn);

        cmd.CommandType = CommandType.StoredProcedure;

        cmd.Parameters.AddWithValue("@Id", (object?)courseId ?? DBNull.Value);

        await conn.OpenAsync();

        using var reader = await cmd.ExecuteReaderAsync();

        var courses = new List<CourseResponse>();

        while (await reader.ReadAsync())
        {
            courses.Add(new CourseResponse
            {
                Id = Convert.ToInt32(reader["Id"]),
                Title = reader["Title"] as string ?? string.Empty,
                Description = reader["Description"] as string,
                IntroVideoUrl = reader["IntroVideoUrl"] as string,
                SlidesJson = reader["SlidesJson"] as string,
                CompletionTimeSeconds = Convert.ToInt32(reader["CompletionTimeSeconds"]),
                PassPercentage = Convert.ToDecimal(reader["PassPercentage"]),
                WwEnvClientId = reader["WwEnvClientId"] as string,
                CourseIconUrl = reader["CourseIconUrl"] as string,
                Tags = reader["Tags"] as string,
                CourseStatus = reader["CourseStatus"] as string ?? string.Empty,
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
                IsActive = Convert.ToBoolean(reader["IsActive"]),
                CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                UpdatedDate = reader["UpdatedDate"] as DateTime?
            });
        }

        if (courseId.HasValue)
            return courses.Count > 0 ? courses[0] : null;

        return courses;
    }

    public async Task<bool> UpdateCourse(
        int courseId,
        UpdateCourseRequest request,
        string? introVideoUrl,
        string? courseIconUrl,
        string editedById,
        string editedByName,
        string editedByRole)
    {
        using var conn = GetConnection();

        using var cmd = new SqlCommand("LMS.SP_UpdateCourse", conn);

        cmd.CommandType = CommandType.StoredProcedure;

        cmd.Parameters.AddWithValue("@Id", courseId);

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
            "@SlidesJson",
            string.IsNullOrWhiteSpace(request.SlidesJson)
                ? DBNull.Value
                : request.SlidesJson);

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
            "@CourseIconUrl",
            string.IsNullOrWhiteSpace(courseIconUrl)
                ? DBNull.Value
                : courseIconUrl);

        cmd.Parameters.AddWithValue(
            "@Tags",
            string.IsNullOrWhiteSpace(request.Tags)
                ? DBNull.Value
                : request.Tags);

        cmd.Parameters.AddWithValue(
            "@CourseStatus",
            string.IsNullOrWhiteSpace(request.CourseStatus)
                ? DBNull.Value
                : request.CourseStatus);

        cmd.Parameters.AddWithValue(
            "@IsActive",
            request.IsActive.HasValue
                ? request.IsActive.Value
                : DBNull.Value);

        cmd.Parameters.AddWithValue("@EditedById", editedById);
        cmd.Parameters.AddWithValue("@EditedByName", editedByName);
        cmd.Parameters.AddWithValue("@EditedByRole", editedByRole);

        await conn.OpenAsync();

        var result = await cmd.ExecuteScalarAsync();

        int rows = Convert.ToInt32(result);

        return rows > 0;
    }

    public async Task<bool> DeleteCourse(
        int courseId,
        string deletedById,
        string deletedByName,
        string deletedByRole)
    {
        using var conn = GetConnection();

        using var cmd = new SqlCommand("LMS.SP_DeleteCourse", conn);

        cmd.CommandType = CommandType.StoredProcedure;

        cmd.Parameters.AddWithValue("@Id", courseId);
        cmd.Parameters.AddWithValue("@DeletedById", deletedById);
        cmd.Parameters.AddWithValue("@DeletedByName", deletedByName);
        cmd.Parameters.AddWithValue("@DeletedByRole", deletedByRole);

        await conn.OpenAsync();

        var result = await cmd.ExecuteScalarAsync();

        int rows = Convert.ToInt32(result);

        return rows > 0;
    }
}
