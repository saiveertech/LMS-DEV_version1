using System.Data;
using LMS.Application.Features.Auth.DTOs;
using LMS.Infrastructure.Email;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace LMS.Infrastructure.Repositories.Trainer;

public class TrainerRepository
{
private readonly IConfiguration _configuration;
private readonly EmailService _emailService;


public TrainerRepository(
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

public async Task RegisterTrainer(RegisterTrainerRequest request)
{
    using var conn = GetConnection();

    await conn.OpenAsync();

    // Get the next Trainer Number
    string countSql = @"
        SELECT ISNULL(MAX(CAST(RIGHT(TrainerId,3) AS INT)),0) + 1
        FROM LMS.Trainers";

    using var countCmd = new SqlCommand(countSql, conn);

    int nextNumber = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

    string firstInitial = request.FirstName.Substring(0, 1).ToUpper();
    string lastInitial = request.LastName.Substring(0, 1).ToUpper();

    string trainerId = $"{firstInitial}{lastInitial}TR{nextNumber:D3}";

    using var cmd = new SqlCommand("LMS.SP_RegisterTrainer", conn);

    cmd.CommandType = CommandType.StoredProcedure;

    cmd.Parameters.AddWithValue("@TrainerId", trainerId);
    cmd.Parameters.AddWithValue("@FirstName", request.FirstName);
    cmd.Parameters.AddWithValue("@LastName", request.LastName);
    cmd.Parameters.AddWithValue("@Email", request.Email);
    cmd.Parameters.AddWithValue("@PhoneNumber", request.PhoneNumber);
    cmd.Parameters.AddWithValue("@Password", BCrypt.Net.BCrypt.HashPassword(request.Password));
    cmd.Parameters.AddWithValue("@ExperienceYears", request.ExperienceYears);
    cmd.Parameters.AddWithValue("@CurrentCompany", (object?)request.CurrentCompany ?? DBNull.Value);
    cmd.Parameters.AddWithValue("@Designation", (object?)request.Designation ?? DBNull.Value);
    cmd.Parameters.AddWithValue("@Bio", (object?)request.Bio ?? DBNull.Value);
    cmd.Parameters.AddWithValue("@LinkedInUrl", (object?)request.LinkedInUrl ?? DBNull.Value);

    await cmd.ExecuteNonQueryAsync();

    await _emailService.SendEmailAsync(
        request.Email,
        request.FirstName,
        "Trainer Registration Successful",
        $@"
        <h2>Welcome {request.FirstName} {request.LastName}</h2>

        <p>Your Trainer registration was successful.</p>

        <p><b>Trainer ID:</b> {trainerId}</p>

        <p>Welcome to SkillToRole LMS.</p>

        <br/>

        <p>
            Regards,<br/>
            SkillToRole Team
        </p>");
}
public async Task<object?> GetTrainerById(
    string trainerId)
{
    using var conn = GetConnection();

    using var cmd =
        new SqlCommand(
            "LMS.SP_GetTrainerById",
            conn);

    cmd.CommandType =
        CommandType.StoredProcedure;

    cmd.Parameters.AddWithValue(
        "@TrainerId",
        trainerId);

    await conn.OpenAsync();

    using var reader =
        await cmd.ExecuteReaderAsync();

    if (await reader.ReadAsync())
    {
        return new
        {
            TrainerId = reader["TrainerId"],
            FirstName = reader["FirstName"],
            LastName = reader["LastName"],
            Email = reader["Email"],
            PhoneNumber = reader["PhoneNumber"],
            ExperienceYears = reader["ExperienceYears"],
            CurrentCompany = reader["CurrentCompany"],
            Designation = reader["Designation"],
            Bio = reader["Bio"],
            LinkedInUrl = reader["LinkedInUrl"]
        };
    }

    return null;
}

public async Task<bool> UpdateTrainer(
    string trainerId,
    UpdateTrainerRequest request)
{
    using var conn = GetConnection();

    using var cmd =
        new SqlCommand(
            "LMS.SP_UpdateTrainer",
            conn);

    cmd.CommandType =
        CommandType.StoredProcedure;

    cmd.Parameters.AddWithValue(
        "@TrainerId",
        trainerId);

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
        "@CurrentCompany",
        (object?)request.CurrentCompany ?? DBNull.Value);

    cmd.Parameters.AddWithValue(
        "@Designation",
        (object?)request.Designation ?? DBNull.Value);

    cmd.Parameters.AddWithValue(
        "@Bio",
        (object?)request.Bio ?? DBNull.Value);

    cmd.Parameters.AddWithValue(
        "@LinkedInUrl",
        (object?)request.LinkedInUrl ?? DBNull.Value);

    await conn.OpenAsync();

    int rows =
        await cmd.ExecuteNonQueryAsync();

    return rows > 0;
}

}
