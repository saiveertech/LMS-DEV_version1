using System.Data;
using LMS.Application.Features.Session.DTOs;
using LMS.Application.Features.Session.Services;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace LMS.Infrastructure.Repositories.Session;

public class SessionRepository : ISessionRepository
{
    private readonly IConfiguration _configuration;

    public SessionRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private SqlConnection GetConnection()
    {
        return new SqlConnection(
            _configuration.GetConnectionString("DefaultConnection"));
    }

    // ─── Create Session ──────────────────────────────────────────────────────

    public async Task CreateSessionAsync(
        string sessionId,
        string userId,
        string role,
        string? deviceInfo)
    {
        using var conn = GetConnection();

        using var cmd = new SqlCommand("LMS.SP_CreateUserSession", conn);

        cmd.CommandType = CommandType.StoredProcedure;

        var expiryMinutes = int.Parse(_configuration["JwtSettings:ExpiryMinutes"]!);

        cmd.Parameters.AddWithValue("@SessionId", sessionId);
        cmd.Parameters.AddWithValue("@UserId", userId);
        cmd.Parameters.AddWithValue("@Role", role);
        cmd.Parameters.AddWithValue("@DeviceInfo", (object?)deviceInfo ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ExpiryMinutes", expiryMinutes);

        await conn.OpenAsync();

        await cmd.ExecuteNonQueryAsync();
    }

    // ─── Get Session By SessionId ────────────────────────────────────────────

    public async Task<SessionResponse?> GetSessionBySessionIdAsync(string sessionId)
    {
        using var conn = GetConnection();

        using var cmd = new SqlCommand("LMS.SP_GetSessionBySessionId", conn);

        cmd.CommandType = CommandType.StoredProcedure;

        cmd.Parameters.AddWithValue("@SessionId", sessionId);

        await conn.OpenAsync();

        using var reader = await cmd.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new SessionResponse
            {
                Id = Convert.ToInt32(reader["Id"]),
                SessionId = reader["SessionId"] as string ?? string.Empty,
                UserId = reader["UserId"] as string ?? string.Empty,
                Role = reader["Role"] as string ?? string.Empty,
                DeviceInfo = reader["DeviceInfo"] as string,
                LoginTime = Convert.ToDateTime(reader["LoginTime"]),
                LogoutTime = reader["LogoutTime"] == DBNull.Value
                    ? null
                    : Convert.ToDateTime(reader["LogoutTime"]),
                IsActive = Convert.ToBoolean(reader["IsActive"])
            };
        }

        return null;
    }

    // ─── Terminate Session ───────────────────────────────────────────────────

    public async Task TerminateSessionAsync(string sessionId)
    {
        using var conn = GetConnection();

        using var cmd = new SqlCommand("LMS.SP_TerminateSession", conn);

        cmd.CommandType = CommandType.StoredProcedure;

        cmd.Parameters.AddWithValue("@SessionId", sessionId);

        await conn.OpenAsync();

        await cmd.ExecuteNonQueryAsync();
    }
}
