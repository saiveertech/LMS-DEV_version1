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

    public async Task<object?> GetCourseById(int courseId)
    {
        using var conn = GetConnection();

        using var cmd = new SqlCommand("LMS.SP_GetCourseById", conn);

        cmd.CommandType = CommandType.StoredProcedure;

        cmd.Parameters.AddWithValue("@Id", courseId);

        await conn.OpenAsync();

        using var reader = await cmd.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new
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
                CreatedAt = reader["CreatedAt"]
            };
        }

        return null;
    }
}
