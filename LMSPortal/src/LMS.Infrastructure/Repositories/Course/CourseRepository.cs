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
        string createdBy,
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

        cmd.Parameters.AddWithValue("@CreatedBy", createdBy);
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

        var courses = new List<object>();

        while (await reader.ReadAsync())
        {
            courses.Add(new
            {
                Id = reader["Id"],
                Title = reader["Title"],
                Description = reader["Description"],
                IntroVideoUrl = reader["IntroVideoUrl"],
                SlidesJson = reader["SlidesJson"],
                CompletionTimeSeconds = reader["CompletionTimeSeconds"],
                PassPercentage = reader["PassPercentage"],
                WwEnvClientId = reader["WwEnvClientId"],
                CourseIconUrl = reader["CourseIconUrl"],
                Tags = reader["Tags"],
                CourseStatus = reader["CourseStatus"],
                CreatedBy = reader["CreatedBy"],
                CreatedByRole = reader["CreatedByRole"],
                IsActive = reader["IsActive"],
                CreatedAt = reader["CreatedAt"],
                UpdatedDate = reader["UpdatedDate"]
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
        string? courseIconUrl)
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

        await conn.OpenAsync();

        var result = await cmd.ExecuteScalarAsync();

        int rows = Convert.ToInt32(result);

        return rows > 0;
    }
}
