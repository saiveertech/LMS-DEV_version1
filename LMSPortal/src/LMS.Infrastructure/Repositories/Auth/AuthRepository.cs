using LMS.Application.Features.Auth.DTOs;
using LMS.Infrastructure.Email;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;

namespace LMS.Infrastructure.Repositories.Auth;

public class AuthRepository
{
private readonly IConfiguration _configuration;
private readonly EmailService _emailService;

public AuthRepository(IConfiguration configuration, EmailService emailService)
{
    _configuration = configuration;
    _emailService = emailService;
}

public SqlConnection GetConnection()
{
    return new SqlConnection(
        _configuration.GetConnectionString("DefaultConnection"));
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

// Looks up the email across Students/Trainers/Admin, and if found,
// generates a 6-digit OTP, stores its hash with a 10-minute expiry,
// and emails it to the user. Silently no-ops if the email isn't found,
// so callers can always return a generic response (avoid leaking
// which emails are registered).
public async Task GenerateAndSendOtp(string email)
{
    using var conn = GetConnection();

    await conn.OpenAsync();

    (string Role, string FirstName)? found = null;

    string studentSql =
        "SELECT FirstName FROM LMS.Students WHERE Email = @Email";

    using (var cmd = new SqlCommand(studentSql, conn))
    {
        cmd.Parameters.AddWithValue("@Email", email);

        var firstName = await cmd.ExecuteScalarAsync() as string;

        if (!string.IsNullOrEmpty(firstName))
            found = ("Student", firstName);
    }

    if (found is null)
    {
        string trainerSql =
            "SELECT FirstName FROM LMS.Trainers WHERE Email = @Email";

        using var cmd = new SqlCommand(trainerSql, conn);

        cmd.Parameters.AddWithValue("@Email", email);

        var firstName = await cmd.ExecuteScalarAsync() as string;

        if (!string.IsNullOrEmpty(firstName))
            found = ("Trainer", firstName);
    }

    if (found is null)
    {
        string adminSql =
            "SELECT FirstName FROM LMS.Admin WHERE Email = @Email";

        using var cmd = new SqlCommand(adminSql, conn);

        cmd.Parameters.AddWithValue("@Email", email);

        var firstName = await cmd.ExecuteScalarAsync() as string;

        if (!string.IsNullOrEmpty(firstName))
            found = ("Admin", firstName);
    }

    if (found is null)
        return;

    string otp = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
    string otpHash = BCrypt.Net.BCrypt.HashPassword(otp);
    var expiresAt = DateTime.UtcNow.AddMinutes(10);

    string invalidateSql = @"
        UPDATE LMS.PasswordResetTokens
        SET IsUsed = 1
        WHERE Email = @Email AND IsUsed = 0";

    using (var invalidateCmd = new SqlCommand(invalidateSql, conn))
    {
        invalidateCmd.Parameters.AddWithValue("@Email", email);

        await invalidateCmd.ExecuteNonQueryAsync();
    }

    string insertSql = @"
        INSERT INTO LMS.PasswordResetTokens (Email, Role, OtpHash, ExpiresAt)
        VALUES (@Email, @Role, @OtpHash, @ExpiresAt)";

    using (var insertCmd = new SqlCommand(insertSql, conn))
    {
        insertCmd.Parameters.AddWithValue("@Email", email);
        insertCmd.Parameters.AddWithValue("@Role", found.Value.Role);
        insertCmd.Parameters.AddWithValue("@OtpHash", otpHash);
        insertCmd.Parameters.AddWithValue("@ExpiresAt", expiresAt);

        await insertCmd.ExecuteNonQueryAsync();
    }

    await _emailService.SendEmailAsync(
        email,
        found.Value.FirstName,
        "Your Password Reset OTP",
        $@"
        <h2>Password Reset Request</h2>

        <p>Hi {found.Value.FirstName},</p>

        <p>Your OTP to reset your password is:</p>

        <h1>{otp}</h1>

        <p>This OTP is valid for 10 minutes. If you didn't request a password reset, you can safely ignore this email.</p>");
}

// Validates the most recent unused OTP for the email. On success, marks
// that email "verified" and extends its expiry window so the user has
// time to submit a new password without re-entering the OTP.
public async Task<(bool Success, string Message)> VerifyOtp(
    string email,
    string otp)
{
    using var conn = GetConnection();

    await conn.OpenAsync();

    string selectSql = @"
        SELECT TOP 1 Id, OtpHash, ExpiresAt, Attempts
        FROM LMS.PasswordResetTokens
        WHERE Email = @Email AND IsUsed = 0
        ORDER BY CreatedDate DESC";

    int tokenId;
    string otpHash;
    DateTime expiresAt;
    int attempts;

    using (var cmd = new SqlCommand(selectSql, conn))
    {
        cmd.Parameters.AddWithValue("@Email", email);

        using var reader = await cmd.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return (false, "Invalid or expired OTP");

        tokenId = reader.GetInt32(0);
        otpHash = reader.GetString(1);
        expiresAt = reader.GetDateTime(2);
        attempts = reader.GetInt32(3);
    }

    if (expiresAt < DateTime.UtcNow)
        return (false, "OTP has expired. Please request a new one.");

    if (attempts >= 5)
        return (false, "Too many failed attempts. Please request a new OTP.");

    if (!BCrypt.Net.BCrypt.Verify(otp, otpHash))
    {
        string incrementSql = @"
            UPDATE LMS.PasswordResetTokens
            SET Attempts = Attempts + 1
            WHERE Id = @Id";

        using var incrementCmd = new SqlCommand(incrementSql, conn);

        incrementCmd.Parameters.AddWithValue("@Id", tokenId);

        await incrementCmd.ExecuteNonQueryAsync();

        return (false, "Invalid OTP");
    }

    var verifiedExpiresAt = DateTime.UtcNow.AddMinutes(15);

    string verifySql = @"
        UPDATE LMS.PasswordResetTokens
        SET IsVerified = 1,
            ExpiresAt = @ExpiresAt
        WHERE Id = @Id";

    using (var verifyCmd = new SqlCommand(verifySql, conn))
    {
        verifyCmd.Parameters.AddWithValue("@ExpiresAt", verifiedExpiresAt);
        verifyCmd.Parameters.AddWithValue("@Id", tokenId);

        await verifyCmd.ExecuteNonQueryAsync();
    }

    return (true, "OTP verified");
}

// Updates the password for whichever table (Student/Trainer/Admin) the
// email belongs to — authorized purely by a prior, unexpired VerifyOtp
// call for that email. No OTP or old password required at this step.
public async Task<(bool Success, string Message)> ResetPasswordAfterVerification(
    string email,
    string newPassword)
{
    using var conn = GetConnection();

    await conn.OpenAsync();

    string selectSql = @"
        SELECT TOP 1 Id, Role, ExpiresAt
        FROM LMS.PasswordResetTokens
        WHERE Email = @Email AND IsUsed = 0 AND IsVerified = 1
        ORDER BY CreatedDate DESC";

    int tokenId;
    string role;
    DateTime expiresAt;

    using (var cmd = new SqlCommand(selectSql, conn))
    {
        cmd.Parameters.AddWithValue("@Email", email);

        using var reader = await cmd.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return (false, "Email not verified. Please verify your OTP first.");

        tokenId = reader.GetInt32(0);
        role = reader.GetString(1);
        expiresAt = reader.GetDateTime(2);
    }

    if (expiresAt < DateTime.UtcNow)
        return (false, "Verification has expired. Please verify your OTP again.");

    string table = role switch
    {
        "Student" => "LMS.Students",
        "Trainer" => "LMS.Trainers",
        "Admin" => "LMS.Admin",
        _ => throw new InvalidOperationException($"Unknown role '{role}' on password reset token")
    };

    string newHash = BCrypt.Net.BCrypt.HashPassword(newPassword);

    string updateSql = $@"
        UPDATE {table}
        SET Password = @Password
        WHERE Email = @Email";

    using (var updateCmd = new SqlCommand(updateSql, conn))
    {
        updateCmd.Parameters.AddWithValue("@Password", newHash);
        updateCmd.Parameters.AddWithValue("@Email", email);

        await updateCmd.ExecuteNonQueryAsync();
    }

    string markUsedSql = @"
        UPDATE LMS.PasswordResetTokens
        SET IsUsed = 1
        WHERE Id = @Id";

    using (var markUsedCmd = new SqlCommand(markUsedSql, conn))
    {
        markUsedCmd.Parameters.AddWithValue("@Id", tokenId);

        await markUsedCmd.ExecuteNonQueryAsync();
    }

    return (true, "Password reset successful");
}

}
