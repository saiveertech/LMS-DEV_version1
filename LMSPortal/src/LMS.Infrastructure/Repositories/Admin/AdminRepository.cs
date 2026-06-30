using System.Data;
using LMS.Application.Features.Admin.DTOs;
using LMS.Application.Features.Auth.Services.Admin;
using LMS.Infrastructure.Email;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace LMS.Infrastructure.Repositories.Admin;

public class AdminRepository : IAdminRepository
{
    private readonly IConfiguration _configuration;
    private readonly EmailService _emailService;

    public AdminRepository(
        IConfiguration configuration,
        EmailService emailService)
    {
        _configuration = configuration;
        _emailService = emailService;
    }

    private SqlConnection GetConnection()
    {
        return new SqlConnection(
            _configuration.GetConnectionString("DefaultConnection"));
    }

    public async Task<object> RegisterAdmin(RegisterAdminRequest request)
    {
        using var conn = GetConnection();

        await conn.OpenAsync();

        // Generate AdminId

        string sql =
            "SELECT ISNULL(MAX(Id),0)+1 FROM LMS.Admin";

        using var countCmd =
            new SqlCommand(sql, conn);

        int nextNumber =
            Convert.ToInt32(await countCmd.ExecuteScalarAsync());

        string adminId =
            $"SK{request.FirstName.Substring(0,1).ToUpper()}" +
            $"{request.LastName.Substring(0,1).ToUpper()}" +
            $"{nextNumber:D3}AD";

        using var cmd =
            new SqlCommand(
                "LMS.SP_RegisterAdmin",
                conn);

        cmd.CommandType =
            CommandType.StoredProcedure;

        cmd.Parameters.AddWithValue(
            "@AdminId",
            adminId);

        cmd.Parameters.AddWithValue(
            "@FirstName",
            request.FirstName);

        cmd.Parameters.AddWithValue(
            "@LastName",
            request.LastName);

        cmd.Parameters.AddWithValue(
            "@Email",
            request.Email);

        cmd.Parameters.AddWithValue(
            "@PhoneNumber",
            request.PhoneNumber);

        cmd.Parameters.AddWithValue(
            "@Password",
            BCrypt.Net.BCrypt.HashPassword(request.Password));

        cmd.Parameters.AddWithValue(
            "@ExperienceYears",
            request.ExperienceYears ?? (object)DBNull.Value);

        cmd.Parameters.AddWithValue(
            "@Skills",
            request.Skills ?? "");

        cmd.Parameters.AddWithValue(
            "@Bio",
            request.Bio ?? "");

        await cmd.ExecuteNonQueryAsync();

        bool emailSent = true;

        string message =
            "Admin Registered Successfully";

        try
        {
            await _emailService.SendEmailAsync(
                request.Email,
                request.FirstName,
                "Admin Registration Successful",
                $@"
                <h2>Welcome {request.FirstName} {request.LastName}</h2>

                <p>Your Admin Registration was successful.</p>

                <p><b>Admin ID :</b> {adminId}</p>

                <br/>

                <p>
                Welcome to SkillToRole LMS
                </p>

                <br/>

                <p>
                Regards,
                <br/>
                SkillToRole Team
                </p>
                ");
        }
        catch
        {
            emailSent = false;

            message =
                "Admin Registered Successfully but Email Sending Failed.";
        }

        return new
        {
            Success = true,
            AdminId = adminId,
            EmailSent = emailSent,
            Message = message
        };
    }

    public async Task<object?> GetAdminById(string adminId)
{
    using var conn = GetConnection();

    using var cmd =
        new SqlCommand(
            "LMS.SP_GetAdminById",
            conn);

    cmd.CommandType =
        CommandType.StoredProcedure;

    cmd.Parameters.AddWithValue(
        "@AdminId",
        adminId);

    await conn.OpenAsync();

    using var reader =
        await cmd.ExecuteReaderAsync();

    if(await reader.ReadAsync())
    {
        return new
        {
            AdminId=reader["AdminId"],
            FirstName=reader["FirstName"],
            LastName=reader["LastName"],
            Email=reader["Email"],
            PhoneNumber=reader["PhoneNumber"],
            ExperienceYears=reader["ExperienceYears"],
            Skills=reader["Skills"],
            Bio=reader["Bio"],
            CreatedDate=reader["CreatedDate"]
        };
    }

    return null;
}

public async Task<bool> UpdateAdmin(
    string adminId,
    UpdateAdminRequest request)
{
    using var conn = GetConnection();

    using var cmd = new SqlCommand(
        "LMS.SP_UpdateAdmin",
        conn);

    cmd.CommandType = CommandType.StoredProcedure;

    cmd.Parameters.AddWithValue(
        "@AdminId",
        adminId);

    cmd.Parameters.AddWithValue(
        "@FirstName",
        (object?)request.FirstName ?? DBNull.Value);

    cmd.Parameters.AddWithValue(
        "@LastName",
        (object?)request.LastName ?? DBNull.Value);

    cmd.Parameters.AddWithValue(
        "@Email",
        (object?)request.Email ?? DBNull.Value);

    cmd.Parameters.AddWithValue(
        "@PhoneNumber",
        (object?)request.PhoneNumber ?? DBNull.Value);

    cmd.Parameters.AddWithValue(
        "@ExperienceYears",
        (object?)request.ExperienceYears ?? DBNull.Value);

    cmd.Parameters.AddWithValue(
        "@Skills",
        (object?)request.Skills ?? DBNull.Value);

    cmd.Parameters.AddWithValue(
        "@Bio",
        (object?)request.Bio ?? DBNull.Value);

    await conn.OpenAsync();

    var result = await cmd.ExecuteScalarAsync();

    int rows = Convert.ToInt32(result);

    return rows > 0;
}
}