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

public async Task<(string Role, string Email)> Login(
    string email,
    string password)
{
    using var conn = GetConnection();

    await conn.OpenAsync();

    // Student

    string studentSql =
        "SELECT Password FROM LMS.Students WHERE Email = @Email";

    using (var cmd =
        new SqlCommand(studentSql, conn))
    {
        cmd.Parameters.AddWithValue(
            "@Email",
            email);

        var hash =
            await cmd.ExecuteScalarAsync() as string;

        if (hash != null &&
            BCrypt.Net.BCrypt.Verify(
                password,
                hash))
        {
            return ("Student", email);
        }
    }

    // Trainer

    string trainerSql =
        "SELECT Password FROM LMS.Trainers WHERE Email = @Email";

    using (var cmd =
        new SqlCommand(trainerSql, conn))
    {
        cmd.Parameters.AddWithValue(
            "@Email",
            email);

        var hash =
            await cmd.ExecuteScalarAsync() as string;

        if (hash != null &&
            BCrypt.Net.BCrypt.Verify(
                password,
                hash))
        {
            return ("Trainer", email);
        }
    }
    // Admin

string adminSql =
    @"SELECT Password
      FROM LMS.Admin
      WHERE Email = @Email";

using (var cmd = new SqlCommand(adminSql, conn))
{
    cmd.Parameters.AddWithValue(
        "@Email",
        email);

    var hash =
        await cmd.ExecuteScalarAsync() as string;

    if (hash != null &&
        BCrypt.Net.BCrypt.Verify(password, hash))
    {
        return ("Admin", email);
    }
}

    return ("", "");
}


}
