using System.Data;
using LMS.Application.Features.Auth.DTOs;
using LMS.Application.Features.Auth.Services.Trainer;
using LMS.Infrastructure.Email;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace LMS.Infrastructure.Repositories.Trainer;

public class TrainerRepository : ITrainerRepository
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

    //=========================================================
    // Register Trainer
    //=========================================================

    public async Task<object> RegisterTrainer(
        RegisterTrainerRequest request)
    {
        using var conn = GetConnection();

        await conn.OpenAsync();

        string sql =
            "SELECT ISNULL(MAX(Id),0)+1 FROM LMS.Trainers";

        using var countCmd =
            new SqlCommand(sql, conn);

        int nextNumber =
            Convert.ToInt32(await countCmd.ExecuteScalarAsync());

        string trainerId =
            $"SK{request.FirstName.Substring(0,1).ToUpper()}" +
            $"{request.LastName.Substring(0,1).ToUpper()}" +
            $"{nextNumber:D3}TR";

        using var cmd =
            new SqlCommand(
                "LMS.SP_RegisterTrainer",
                conn);

        cmd.CommandType =
            CommandType.StoredProcedure;

        cmd.Parameters.AddWithValue("@TrainerId", trainerId);
        cmd.Parameters.AddWithValue("@FirstName", request.FirstName);
        cmd.Parameters.AddWithValue("@LastName", request.LastName);
        cmd.Parameters.AddWithValue("@Email", request.Email);
        cmd.Parameters.AddWithValue("@PhoneNumber", request.PhoneNumber);
        cmd.Parameters.AddWithValue(
            "@Password",
            BCrypt.Net.BCrypt.HashPassword(request.Password));

        cmd.Parameters.AddWithValue(
            "@ExperienceYears",
            request.ExperienceYears);

        cmd.Parameters.AddWithValue(
            "@CurrentCompany",
            request.CurrentCompany ?? "");

        cmd.Parameters.AddWithValue(
            "@Designation",
            request.Designation ?? "");

        cmd.Parameters.AddWithValue(
            "@Bio",
            request.Bio ?? "");

        cmd.Parameters.AddWithValue(
            "@LinkedInUrl",
            request.LinkedInUrl ?? "");

        await cmd.ExecuteNonQueryAsync();

        bool emailSent = true;

        string message =
            "Trainer Registered Successfully";

        try
        {
            await _emailService.SendEmailAsync(
                request.Email,
                request.FirstName,
                "Trainer Registration Successful",
                $@"
                <h2>Welcome {request.FirstName} {request.LastName}</h2>

                <p>Your Trainer Registration was successful.</p>

                <p><b>Trainer ID :</b> {trainerId}</p>

                <br/>

                <p>Welcome to SkillToRole LMS</p>

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
                "Trainer Registered Successfully but Email Sending Failed.";
        }

        return new
        {
            Success = true,
            TrainerId = trainerId,
            EmailSent = emailSent,
            Message = message
        };
    }

    //=========================================================
    // Get Trainer By Id
    //=========================================================

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
                LinkedInUrl = reader["LinkedInUrl"],
                CreatedDate = reader["CreatedDate"]
            };
        }

        return null;
    }

    //=========================================================
    // Update Trainer
    //=========================================================

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
            string.IsNullOrWhiteSpace(request.FirstName)
                ? DBNull.Value
                : request.FirstName);

        cmd.Parameters.AddWithValue(
            "@LastName",
            string.IsNullOrWhiteSpace(request.LastName)
                ? DBNull.Value
                : request.LastName);

        cmd.Parameters.AddWithValue(
            "@Email",
            string.IsNullOrWhiteSpace(request.Email)
                ? DBNull.Value
                : request.Email);

        cmd.Parameters.AddWithValue(
            "@PhoneNumber",
            string.IsNullOrWhiteSpace(request.PhoneNumber)
                ? DBNull.Value
                : request.PhoneNumber);

        cmd.Parameters.AddWithValue(
            "@ExperienceYears",
            request.ExperienceYears.HasValue
                ? request.ExperienceYears.Value
                : DBNull.Value);

        cmd.Parameters.AddWithValue(
            "@CurrentCompany",
            string.IsNullOrWhiteSpace(request.CurrentCompany)
                ? DBNull.Value
                : request.CurrentCompany);

        cmd.Parameters.AddWithValue(
            "@Designation",
            string.IsNullOrWhiteSpace(request.Designation)
                ? DBNull.Value
                : request.Designation);

        cmd.Parameters.AddWithValue(
            "@Bio",
            string.IsNullOrWhiteSpace(request.Bio)
                ? DBNull.Value
                : request.Bio);

        cmd.Parameters.AddWithValue(
            "@LinkedInUrl",
            string.IsNullOrWhiteSpace(request.LinkedInUrl)
                ? DBNull.Value
                : request.LinkedInUrl);

        await conn.OpenAsync();

        var result =
            await cmd.ExecuteScalarAsync();

        int rows =
            Convert.ToInt32(result);

        return rows > 0;
    }
}