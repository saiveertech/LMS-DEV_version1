using LMS.Application.Features.Auth.DTOs;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace LMS.Infrastructure.Repositories.Auth;

public class AuthRepository
{
private readonly IConfiguration _configuration;

public AuthRepository(IConfiguration configuration)
{
    _configuration = configuration;
}

public SqlConnection GetConnection()
{
    return new SqlConnection(
        _configuration.GetConnectionString("DefaultConnection"));
}

public async Task<bool> ResetPassword(
    string email,
    string oldPassword,
    string newPassword)
{
    using var conn = GetConnection();

    await conn.OpenAsync();

    // Student

    string studentSql = @"
        SELECT Password
        FROM LMS.Students
        WHERE Email = @Email";

    using (var cmd =
        new SqlCommand(studentSql, conn))
    {
        cmd.Parameters.AddWithValue(
            "@Email",
            email);

        var storedPassword =
            await cmd.ExecuteScalarAsync() as string;

        if (!string.IsNullOrEmpty(storedPassword) &&
            BCrypt.Net.BCrypt.Verify(
                oldPassword,
                storedPassword))
        {
            string newHash =
                BCrypt.Net.BCrypt.HashPassword(
                    newPassword);

            string updateSql = @"
                UPDATE LMS.Students
                SET Password = @Password
                WHERE Email = @Email";

            using var updateCmd =
                new SqlCommand(updateSql, conn);

            updateCmd.Parameters.AddWithValue(
                "@Email",
                email);

            updateCmd.Parameters.AddWithValue(
                "@Password",
                newHash);

            await updateCmd.ExecuteNonQueryAsync();

            return true;
        }
    }

    // Trainer

    string trainerSql = @"
        SELECT Password
        FROM LMS.Trainers
        WHERE Email = @Email";

    using (var cmd =
        new SqlCommand(trainerSql, conn))
    {
        cmd.Parameters.AddWithValue(
            "@Email",
            email);

        var storedPassword =
            await cmd.ExecuteScalarAsync() as string;

        if (!string.IsNullOrEmpty(storedPassword) &&
            BCrypt.Net.BCrypt.Verify(
                oldPassword,
                storedPassword))
        {
            string newHash =
                BCrypt.Net.BCrypt.HashPassword(
                    newPassword);

            string updateSql = @"
                UPDATE LMS.Trainers
                SET Password = @Password
                WHERE Email = @Email";

            using var updateCmd =
                new SqlCommand(updateSql, conn);

            updateCmd.Parameters.AddWithValue(
                "@Email",
                email);

            updateCmd.Parameters.AddWithValue(
                "@Password",
                newHash);

            await updateCmd.ExecuteNonQueryAsync();

            return true;
        }
    }
    // Admin

string adminSql = @"
    SELECT Password
    FROM LMS.Admin
    WHERE Email = @Email";

using (var cmd =
    new SqlCommand(adminSql, conn))
{
    cmd.Parameters.AddWithValue(
        "@Email",
        email);

    var storedPassword =
        await cmd.ExecuteScalarAsync() as string;

    if (!string.IsNullOrEmpty(storedPassword) &&
        BCrypt.Net.BCrypt.Verify(
            oldPassword,
            storedPassword))
    {
        string newHash =
            BCrypt.Net.BCrypt.HashPassword(
                newPassword);

        string updateSql = @"
            UPDATE LMS.Admin
            SET Password = @Password
            WHERE Email = @Email";

        using var updateCmd =
            new SqlCommand(updateSql, conn);

        updateCmd.Parameters.AddWithValue(
            "@Email",
            email);

        updateCmd.Parameters.AddWithValue(
            "@Password",
            newHash);

        await updateCmd.ExecuteNonQueryAsync();

        return true;
    }
}

    return false;
}

public async Task<(string Role, string Email, string Id, string Name)> Login(
    string email,
    string password)
{
    using var conn = GetConnection();

    await conn.OpenAsync();

    // Student

    string studentSql =
        "SELECT StudentId, FirstName, LastName, Password FROM LMS.Students WHERE Email = @Email";

    using (var cmd =
        new SqlCommand(studentSql, conn))
    {
        cmd.Parameters.AddWithValue(
            "@Email",
            email);

        using var reader = await cmd.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            var studentId = reader.GetString(0);
            var name = $"{reader.GetString(1)} {reader.GetString(2)}";
            var hash = reader.GetString(3);

            if (BCrypt.Net.BCrypt.Verify(password, hash))
            {
                return ("Student", email, studentId, name);
            }
        }
    }

    // Trainer

    string trainerSql =
        "SELECT TrainerId, FirstName, LastName, Password FROM LMS.Trainers WHERE Email = @Email";

    using (var cmd =
        new SqlCommand(trainerSql, conn))
    {
        cmd.Parameters.AddWithValue(
            "@Email",
            email);

        using var reader = await cmd.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            var trainerId = reader.GetString(0);
            var name = $"{reader.GetString(1)} {reader.GetString(2)}";
            var hash = reader.GetString(3);

            if (BCrypt.Net.BCrypt.Verify(password, hash))
            {
                return ("Trainer", email, trainerId, name);
            }
        }
    }
    // Admin

string adminSql =
    @"SELECT AdminId, FirstName, LastName, Password
      FROM LMS.Admin
      WHERE Email = @Email";

using (var cmd = new SqlCommand(adminSql, conn))
{
    cmd.Parameters.AddWithValue(
        "@Email",
        email);

    using var reader = await cmd.ExecuteReaderAsync();

    if (await reader.ReadAsync())
    {
        var adminId = reader.GetString(0);
        var name = $"{reader.GetString(1)} {reader.GetString(2)}";
        var hash = reader.GetString(3);

        if (BCrypt.Net.BCrypt.Verify(password, hash))
        {
            return ("Admin", email, adminId, name);
        }
    }
}

    return ("", "", "", "");
}


}
