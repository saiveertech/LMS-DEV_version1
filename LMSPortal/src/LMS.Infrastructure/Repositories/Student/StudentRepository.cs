using System.Data;
using LMS.Application.Features.Auth.DTOs;
using LMS.Infrastructure.Email;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace LMS.Infrastructure.Repositories.Student;

public class StudentRepository
{
private readonly IConfiguration _configuration;
private readonly EmailService _emailService;
public StudentRepository(
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

public async Task<object> RegisterStudent(RegisterStudentRequest request)
{
    using var conn = GetConnection();

    await conn.OpenAsync();

    string countSql = "SELECT ISNULL(MAX(Id),0) + 1 FROM LMS.Students";

    using var countCmd = new SqlCommand(countSql, conn);

    int nextNumber = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

    string studentId =
        $"SK{request.FirstName.Substring(0, 1).ToUpper()}" +
        $"{request.LastName.Substring(0, 1).ToUpper()}" +
        $"{nextNumber:D3}SD";

    using var cmd = new SqlCommand("LMS.SP_RegisterStudent", conn);

    cmd.CommandType = CommandType.StoredProcedure;

    cmd.Parameters.AddWithValue("@StudentId", studentId);
    cmd.Parameters.AddWithValue("@FirstName", request.FirstName);
    cmd.Parameters.AddWithValue("@LastName", request.LastName);
    cmd.Parameters.AddWithValue("@Email", request.Email);
    cmd.Parameters.AddWithValue("@PhoneNumber", request.PhoneNumber);
    cmd.Parameters.AddWithValue("@Password", BCrypt.Net.BCrypt.HashPassword(request.Password));
    cmd.Parameters.AddWithValue("@EducationDetails", request.EducationDetails ?? "");
    cmd.Parameters.AddWithValue("@AreaOfInterest", request.AreaOfInterest ?? "");

    // Save student to database
    await cmd.ExecuteNonQueryAsync();

    bool emailSent = true;
    string message = "Student Registered Successfully";

    try
    {
        await _emailService.SendEmailAsync(
            request.Email,
            request.FirstName,
            "Student Registration Successful",
            $@"
            <h2>Welcome {request.FirstName} {request.LastName}</h2>

            <p>Your registration was successful.</p>

            <p><b>Student ID:</b> {studentId}</p>

            <p>Welcome to SkillToRole LMS.</p>

            <br/>

            <p>Regards,<br/>SkillToRole Team</p>
            ");
    }
    catch (Exception ex)
    {
        emailSent = false;
        message = "Student registered successfully, but email sending failed.";

        // Optional: Log the error
        Console.WriteLine(ex.ToString());
    }

    return new
    {
        Success = true,
        StudentId = studentId,
        EmailSent = emailSent,
        Message = message
    };
}
public async Task<object?> GetStudentById(
    string studentId)
{
    using var conn = GetConnection();

    using var cmd =
        new SqlCommand(
            "LMS.SP_GetStudentById",
            conn);

    cmd.CommandType =
        CommandType.StoredProcedure;

    cmd.Parameters.AddWithValue(
        "@StudentId",
        studentId);

    await conn.OpenAsync();

    using var reader =
        await cmd.ExecuteReaderAsync();

    if (await reader.ReadAsync())
    {
        return new
        {
            StudentId = reader["StudentId"],
            FirstName = reader["FirstName"],
            LastName = reader["LastName"],
            Email = reader["Email"],
            PhoneNumber = reader["PhoneNumber"],
            EducationDetails = reader["EducationDetails"],
            AreaOfInterest = reader["AreaOfInterest"],
            CreatedDate = reader["CreatedDate"]
        };
    }

    return null;
}

public async Task<bool> UpdateStudent(
    string studentId,
    RegisterStudentRequest request)
{
    using var conn = GetConnection();

    using var cmd =
        new SqlCommand(
            "LMS.SP_UpdateStudent",
            conn);

    cmd.CommandType =
        CommandType.StoredProcedure;

    cmd.Parameters.AddWithValue(
        "@StudentId",
        studentId);

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
        "@EducationDetails",
        request.EducationDetails ?? "");

    cmd.Parameters.AddWithValue(
        "@AreaOfInterest",
        request.AreaOfInterest ?? "");

    await conn.OpenAsync();

    int rows =
        await cmd.ExecuteNonQueryAsync();

    return rows > 0;
}


}
