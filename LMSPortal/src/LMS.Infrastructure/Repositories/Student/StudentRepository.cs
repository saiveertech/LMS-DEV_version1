using System.Data;
using LMS.Application.Features.Auth.DTOs;
using LMS.Application.Features.Auth.Services.Student;
using LMS.Infrastructure.Email;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace LMS.Infrastructure.Repositories.Student;

public class StudentRepository : IStudentRepository
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

    using var cmd = new SqlCommand("LMS.SP_RegisterStudent", conn);

    cmd.CommandType = CommandType.StoredProcedure;

    var studentIdParam =
        new SqlParameter("@StudentId", SqlDbType.NVarChar, 50)
        {
            Direction = ParameterDirection.Output
        };

    cmd.Parameters.Add(studentIdParam);
    cmd.Parameters.AddWithValue("@FirstName", request.FirstName);
    cmd.Parameters.AddWithValue("@LastName", request.LastName);
    cmd.Parameters.AddWithValue("@Email", request.Email);
    cmd.Parameters.AddWithValue("@PhoneNumber", request.PhoneNumber);
    cmd.Parameters.AddWithValue("@Password", BCrypt.Net.BCrypt.HashPassword(request.Password));
    cmd.Parameters.AddWithValue("@EducationDetails", request.EducationDetails ?? "");
    cmd.Parameters.AddWithValue("@AreaOfInterest", request.AreaOfInterest ?? "");

    // Save student to database
    await cmd.ExecuteNonQueryAsync();

    string studentId = (string)studentIdParam.Value;

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
    UpdateStudentRequest request)
{
    using var conn = GetConnection();

    using var cmd = new SqlCommand(
        "LMS.SP_UpdateStudent",
        conn);

    cmd.CommandType = CommandType.StoredProcedure;

    cmd.Parameters.AddWithValue("@StudentId", studentId);

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
        "@EducationDetails",
        string.IsNullOrWhiteSpace(request.EducationDetails)
            ? DBNull.Value
            : request.EducationDetails);

    cmd.Parameters.AddWithValue(
        "@AreaOfInterest",
        string.IsNullOrWhiteSpace(request.AreaOfInterest)
            ? DBNull.Value
            : request.AreaOfInterest);

    await conn.OpenAsync();

    var result = await cmd.ExecuteScalarAsync();

    int rows = Convert.ToInt32(result);

    return rows > 0;
}

}
