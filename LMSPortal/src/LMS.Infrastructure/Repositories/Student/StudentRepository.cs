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
    string? studentId = null)
{
    using var conn = GetConnection();

    string sql = @"
        SELECT StudentId, FirstName, LastName, Email, PhoneNumber,
               EducationDetails, AreaOfInterest, CreatedDate
        FROM LMS.Students
        WHERE (@StudentId IS NULL OR StudentId = @StudentId)
        ORDER BY CreatedDate DESC";

    using var cmd = new SqlCommand(sql, conn);

    cmd.Parameters.AddWithValue(
        "@StudentId",
        (object?)studentId ?? DBNull.Value);

    await conn.OpenAsync();

    using var reader =
        await cmd.ExecuteReaderAsync();

    var students = new List<object>();

    while (await reader.ReadAsync())
    {
        students.Add(new
        {
            StudentId = reader["StudentId"],
            FirstName = reader["FirstName"],
            LastName = reader["LastName"],
            Email = reader["Email"],
            PhoneNumber = reader["PhoneNumber"],
            EducationDetails = reader["EducationDetails"],
            AreaOfInterest = reader["AreaOfInterest"],
            CreatedDate = reader["CreatedDate"]
        });
    }

    if (studentId != null)
        return students.Count > 0 ? students[0] : null;

    return students;
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

public async Task<object> EnrollCourse(EnrollCourseRequest request)
{
    using var conn = GetConnection();

    using var cmd = new SqlCommand("LMS.SP_EnrollStudentCourse", conn);

    cmd.CommandType = CommandType.StoredProcedure;

    var enrollmentIdParam =
        new SqlParameter("@EnrollmentId", SqlDbType.NVarChar, 50)
        {
            Direction = ParameterDirection.Output
        };

    cmd.Parameters.Add(enrollmentIdParam);
    cmd.Parameters.AddWithValue("@StudentId", request.StudentId);
    cmd.Parameters.AddWithValue("@CourseId", request.CourseId);

    await conn.OpenAsync();

    await cmd.ExecuteNonQueryAsync();

    string enrollmentId = (string)enrollmentIdParam.Value;

    return new
    {
        EnrollmentId = enrollmentId,
        StudentId = request.StudentId,
        CourseId = request.CourseId,
        CourseStatus = "Enrolled",
        CertificateStatus = "Pending",
        EnrollmentDate = DateTime.UtcNow
    };
}

public async Task<object?> GetEnrolledCourses(string studentId)
{
    using var conn = GetConnection();

    using var cmd = new SqlCommand("LMS.SP_GetStudentEnrolledCourses", conn);

    cmd.CommandType = CommandType.StoredProcedure;

    cmd.Parameters.AddWithValue("@StudentId", studentId);

    await conn.OpenAsync();

    using var reader = await cmd.ExecuteReaderAsync();

    var enrollments = new List<object>();

    while (await reader.ReadAsync())
    {
        enrollments.Add(new
        {
            EnrollmentId = reader["EnrollmentId"],
            StudentId = reader["StudentId"],
            CourseId = reader["CourseId"],
            CourseTitle = reader["CourseTitle"],
            CourseDescription = reader["CourseDescription"] as string,
            CourseIconUrl = reader["CourseIconUrl"] as string,
            Tags = reader["Tags"] as string,
            CompletionTimeSeconds = Convert.ToInt32(reader["CompletionTimeSeconds"]),
            EnrollmentDate = Convert.ToDateTime(reader["EnrollmentDate"]),
            CourseStatus = reader["CourseStatus"],
            CertificateStatus = reader["CertificateStatus"],
            CertificateIssueDate = reader["CertificateIssueDate"] == DBNull.Value
                ? (DateTime?)null
                : Convert.ToDateTime(reader["CertificateIssueDate"]),
            CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
            UpdatedDate = reader["UpdatedDate"] == DBNull.Value
                ? (DateTime?)null
                : Convert.ToDateTime(reader["UpdatedDate"])
        });
    }

    return enrollments;
}

public async Task<object> EnrollAssignment(EnrollAssignmentRequest request)
{
    using var conn = GetConnection();

    using var cmd = new SqlCommand("LMS.SP_EnrollStudentAssignment", conn);

    cmd.CommandType = CommandType.StoredProcedure;

    var enrollmentIdParam =
        new SqlParameter("@EnrollmentId", SqlDbType.NVarChar, 50)
        {
            Direction = ParameterDirection.Output
        };

    cmd.Parameters.Add(enrollmentIdParam);
    cmd.Parameters.AddWithValue("@StudentId", request.StudentId);
    cmd.Parameters.AddWithValue("@AssignmentId", request.AssignmentId);

    await conn.OpenAsync();

    await cmd.ExecuteNonQueryAsync();

    string enrollmentId = (string)enrollmentIdParam.Value;

    return new
    {
        EnrollmentId = enrollmentId,
        StudentId = request.StudentId,
        AssignmentId = request.AssignmentId,
        AssignmentStatus = "Enrolled",
        EnrollmentDate = DateTime.UtcNow
    };
}

public async Task<object?> GetEnrolledAssignments(string studentId)
{
    using var conn = GetConnection();

    using var cmd = new SqlCommand("LMS.SP_GetStudentEnrolledAssignments", conn);

    cmd.CommandType = CommandType.StoredProcedure;

    cmd.Parameters.AddWithValue("@StudentId", studentId);

    await conn.OpenAsync();

    using var reader = await cmd.ExecuteReaderAsync();

    var enrollments = new List<object>();

    while (await reader.ReadAsync())
    {
        enrollments.Add(new
        {
            EnrollmentId = reader["EnrollmentId"],
            StudentId = reader["StudentId"],
            AssignmentId = reader["AssignmentId"],
            AssignmentTitle = reader["AssignmentTitle"],
            AssignmentDescription = reader["AssignmentDescription"] as string,
            AssessmentIconUrl = reader["AssessmentIconUrl"] as string,
            Tags = reader["Tags"] as string,
            CompletionTimeSeconds = Convert.ToInt32(reader["CompletionTimeSeconds"]),
            PassPercentage = Convert.ToDecimal(reader["PassPercentage"]),
            EnrollmentDate = Convert.ToDateTime(reader["EnrollmentDate"]),
            AssignmentStatus = reader["AssignmentStatus"],
            AssessmentScore = reader["AssessmentScore"] == DBNull.Value
                ? (decimal?)null
                : Convert.ToDecimal(reader["AssessmentScore"]),
            CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
            UpdatedDate = reader["UpdatedDate"] == DBNull.Value
                ? (DateTime?)null
                : Convert.ToDateTime(reader["UpdatedDate"])
        });
    }

    return enrollments;
}

}
