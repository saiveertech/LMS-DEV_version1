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
        var idleTimeoutMinutes = int.Parse(_configuration["SessionSettings:IdleTimeoutMinutes"]!);

        cmd.Parameters.AddWithValue("@SessionId", sessionId);
        cmd.Parameters.AddWithValue("@UserId", userId);
        cmd.Parameters.AddWithValue("@Role", role);
        cmd.Parameters.AddWithValue("@DeviceInfo", (object?)deviceInfo ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ExpiryMinutes", expiryMinutes);
        cmd.Parameters.AddWithValue("@IdleTimeoutMinutes", idleTimeoutMinutes);

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

    // ─── Validate And Touch Session ──────────────────────────────────────────

    public async Task<bool> ValidateAndTouchSessionAsync(string sessionId)
    {
        using var conn = GetConnection();

        using var cmd = new SqlCommand("LMS.SP_ValidateAndTouchSession", conn);

        cmd.CommandType = CommandType.StoredProcedure;

        var idleTimeoutMinutes = int.Parse(_configuration["SessionSettings:IdleTimeoutMinutes"]!);

        cmd.Parameters.AddWithValue("@SessionId", sessionId);
        cmd.Parameters.AddWithValue("@IdleTimeoutMinutes", idleTimeoutMinutes);

        await conn.OpenAsync();

        var result = await cmd.ExecuteScalarAsync();

        return result != null && Convert.ToBoolean(result);
    }

    // ─── Expire Stale Sessions ───────────────────────────────────────────────

    public async Task<int> ExpireStaleSessionsAsync()
    {
        using var conn = GetConnection();

        using var cmd = new SqlCommand("LMS.SP_ExpireStaleSessions", conn);

        cmd.CommandType = CommandType.StoredProcedure;

        var expiryMinutes = int.Parse(_configuration["JwtSettings:ExpiryMinutes"]!);
        var idleTimeoutMinutes = int.Parse(_configuration["SessionSettings:IdleTimeoutMinutes"]!);

        cmd.Parameters.AddWithValue("@IdleTimeoutMinutes", idleTimeoutMinutes);
        cmd.Parameters.AddWithValue("@ExpiryMinutes", expiryMinutes);

        await conn.OpenAsync();

        var result = await cmd.ExecuteScalarAsync();

        return result == null ? 0 : Convert.ToInt32(result);
    }

    // ─── Deactivate All Sessions For User ────────────────────────────────────

    public async Task DeactivateAllSessionsForUserAsync(string userId)
    {
        using var conn = GetConnection();

        using var cmd = new SqlCommand("LMS.SP_DeactivateAllSessionsForUser", conn);

        cmd.CommandType = CommandType.StoredProcedure;

        cmd.Parameters.AddWithValue("@UserId", userId);

        await conn.OpenAsync();

        await cmd.ExecuteNonQueryAsync();
    }
}
