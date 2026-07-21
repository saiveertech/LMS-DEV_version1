using System.Data;
using System.Text.Json;
using LMS.Application.Features.Auth.DTOs;
using LMS.Application.Features.Auth.Services.Student;
using LMS.Application.Features.CourseStudentTracking.DTOs;
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
            Attempts = Convert.ToInt32(reader["Attempts"]),
            CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
            UpdatedDate = reader["UpdatedDate"] == DBNull.Value
                ? (DateTime?)null
                : Convert.ToDateTime(reader["UpdatedDate"])
        });
    }

    return enrollments;
}

public async Task<object> SubmitAssignmentAttempt(SubmitAssignmentAttemptRequest request)
{
    using var conn = GetConnection();

    using var cmd = new SqlCommand("LMS.SP_SubmitAssignmentAttempt", conn);

    cmd.CommandType = CommandType.StoredProcedure;

    cmd.Parameters.AddWithValue("@StudentId", request.StudentId);
    cmd.Parameters.AddWithValue("@AssignmentId", request.AssignmentId);
    cmd.Parameters.AddWithValue("@Score", request.Score);

    await conn.OpenAsync();

    using var reader = await cmd.ExecuteReaderAsync();

    if (await reader.ReadAsync())
    {
        return new
        {
            EnrollmentId = reader["EnrollmentId"],
            StudentId = reader["StudentId"],
            AssignmentId = reader["AssignmentId"],
            AssignmentTitle = reader["AssignmentTitle"],
            PassPercentage = Convert.ToDecimal(reader["PassPercentage"]),
            EnrollmentDate = Convert.ToDateTime(reader["EnrollmentDate"]),
            AssignmentStatus = reader["AssignmentStatus"],
            AssessmentScore = Convert.ToDecimal(reader["AssessmentScore"]),
            Attempts = Convert.ToInt32(reader["Attempts"]),
            CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
            UpdatedDate = reader["UpdatedDate"] == DBNull.Value
                ? (DateTime?)null
                : Convert.ToDateTime(reader["UpdatedDate"])
        };
    }

    throw new InvalidOperationException("Failed to record assignment attempt.");
}

public async Task RecordCourseAssignment(
    string studentId, int courseId,
    string assignedById, string assignedByName, string assignedByRole)
{
    using var conn = GetConnection();

    using var cmd = new SqlCommand("LMS.SP_RecordCourseAssignment", conn);

    cmd.CommandType = CommandType.StoredProcedure;

    cmd.Parameters.AddWithValue("@StudentId", studentId);
    cmd.Parameters.AddWithValue("@CourseId", courseId);
    cmd.Parameters.AddWithValue("@AssignedById", assignedById);
    cmd.Parameters.AddWithValue("@AssignedByName", assignedByName);
    cmd.Parameters.AddWithValue("@AssignedByRole", assignedByRole);

    await conn.OpenAsync();

    await cmd.ExecuteNonQueryAsync();
}

public async Task RecordAssignmentAllocation(
    string studentId, int assignmentId,
    string assignedById, string assignedByName, string assignedByRole)
{
    using var conn = GetConnection();

    using var cmd = new SqlCommand("LMS.SP_RecordAssignmentAllocation", conn);

    cmd.CommandType = CommandType.StoredProcedure;

    cmd.Parameters.AddWithValue("@StudentId", studentId);
    cmd.Parameters.AddWithValue("@AssignmentId", assignmentId);
    cmd.Parameters.AddWithValue("@AssignedById", assignedById);
    cmd.Parameters.AddWithValue("@AssignedByName", assignedByName);
    cmd.Parameters.AddWithValue("@AssignedByRole", assignedByRole);

    await conn.OpenAsync();

    await cmd.ExecuteNonQueryAsync();
}

public async Task<object?> GetAssignedCourses(string studentId)
{
    using var conn = GetConnection();

    using var cmd = new SqlCommand("LMS.SP_GetCourseAssignmentsByStudent", conn);

    cmd.CommandType = CommandType.StoredProcedure;

    cmd.Parameters.AddWithValue("@StudentId", studentId);

    await conn.OpenAsync();

    using var reader = await cmd.ExecuteReaderAsync();

    var assignments = new List<object>();

    while (await reader.ReadAsync())
    {
        assignments.Add(new
        {
            AssignmentRecordId = reader["AssignmentRecordId"],
            StudentId = reader["StudentId"],
            CourseId = reader["CourseId"],
            CourseTitle = reader["CourseTitle"],
            CourseDescription = reader["CourseDescription"] as string,
            CourseIconUrl = reader["CourseIconUrl"] as string,
            Tags = reader["Tags"] as string,
            CompletionTimeSeconds = Convert.ToInt32(reader["CompletionTimeSeconds"]),
            AssignedById = reader["AssignedById"],
            AssignedByName = reader["AssignedByName"],
            AssignedByRole = reader["AssignedByRole"],
            AssignedDate = Convert.ToDateTime(reader["AssignedDate"])
        });
    }

    return assignments;
}

public async Task<object?> GetAssignedAssignments(string studentId)
{
    using var conn = GetConnection();

    using var cmd = new SqlCommand("LMS.SP_GetAssignmentAllocationsByStudent", conn);

    cmd.CommandType = CommandType.StoredProcedure;

    cmd.Parameters.AddWithValue("@StudentId", studentId);

    await conn.OpenAsync();

    using var reader = await cmd.ExecuteReaderAsync();

    var allocations = new List<object>();

    while (await reader.ReadAsync())
    {
        allocations.Add(new
        {
            AllocationRecordId = reader["AllocationRecordId"],
            StudentId = reader["StudentId"],
            AssignmentId = reader["AssignmentId"],
            AssignmentTitle = reader["AssignmentTitle"],
            AssignmentDescription = reader["AssignmentDescription"] as string,
            AssessmentIconUrl = reader["AssessmentIconUrl"] as string,
            Tags = reader["Tags"] as string,
            CompletionTimeSeconds = Convert.ToInt32(reader["CompletionTimeSeconds"]),
            PassPercentage = Convert.ToDecimal(reader["PassPercentage"]),
            AssignedById = reader["AssignedById"],
            AssignedByName = reader["AssignedByName"],
            AssignedByRole = reader["AssignedByRole"],
            AssignedDate = Convert.ToDateTime(reader["AssignedDate"])
        });
    }

    return allocations;
}

public async Task<CourseStudentTrackingResponse?> CompleteCourseSlide(string studentId, int slideId)
{
    using var conn = GetConnection();

    using var cmd = new SqlCommand("LMS.SP_CompleteCourseSlide", conn);

    cmd.CommandType = CommandType.StoredProcedure;

    cmd.Parameters.AddWithValue("@StudentId", studentId);
    cmd.Parameters.AddWithValue("@SlideId", slideId);

    await conn.OpenAsync();

    using var reader = await cmd.ExecuteReaderAsync();

    if (await reader.ReadAsync())
    {
        return new CourseStudentTrackingResponse
        {
            StudentId = reader["StudentId"] as string ?? string.Empty,
            StudentName = reader["StudentName"] as string ?? string.Empty,
            Email = reader["Email"] as string ?? string.Empty,
            CourseId = Convert.ToInt32(reader["CourseId"]),
            CourseTitle = reader["CourseTitle"] as string ?? string.Empty,
            EnrollmentDate = Convert.ToDateTime(reader["EnrollmentDate"]),
            EnrollmentSource = reader["EnrollmentSource"] as string ?? string.Empty,
            AssignedByName = reader["AssignedByName"] as string,
            RegistrationStatus = reader["RegistrationStatus"] as string ?? string.Empty,
            CourseStatus = reader["CourseStatus"] as string ?? string.Empty,
            CompletionPercentage = Convert.ToDecimal(reader["CompletionPercentage"]),
            CompletedLessons = Convert.ToInt32(reader["CompletedLessons"]),
            TotalLessons = Convert.ToInt32(reader["TotalLessons"]),
            AssessmentScore = reader["AssessmentScore"] == DBNull.Value
                ? null
                : Convert.ToDecimal(reader["AssessmentScore"]),
            CertificateGenerated = Convert.ToBoolean(reader["CertificateGenerated"]),
            CertificateId = reader["CertificateId"] as string,
            CertificateIssueDate = reader["CertificateIssueDate"] == DBNull.Value
                ? null
                : Convert.ToDateTime(reader["CertificateIssueDate"]),
            CertificateUrl = reader["CertificateUrl"] as string,
            CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
            UpdatedDate = reader["UpdatedDate"] == DBNull.Value
                ? null
                : Convert.ToDateTime(reader["UpdatedDate"])
        };
    }

    return null;
}

public async Task<List<CourseSlideProgressResponse>> GetCourseResume(string studentId, int courseId)
{
    using var conn = GetConnection();

    using var cmd = new SqlCommand("LMS.SP_GetCourseResume", conn);

    cmd.CommandType = CommandType.StoredProcedure;

    cmd.Parameters.AddWithValue("@StudentId", studentId);
    cmd.Parameters.AddWithValue("@CourseId", courseId);

    await conn.OpenAsync();

    using var reader = await cmd.ExecuteReaderAsync();

    var slides = new List<CourseSlideProgressResponse>();

    while (await reader.ReadAsync())
    {
        slides.Add(new CourseSlideProgressResponse
        {
            SlideId = Convert.ToInt32(reader["SlideId"]),
            Title = reader["Title"] as string ?? string.Empty,
            MediaType = reader["MediaType"] as string ?? string.Empty,
            MediaUrl = reader["MediaUrl"] as string ?? string.Empty,
            SortOrder = Convert.ToInt32(reader["SortOrder"]),
            IsCompleted = Convert.ToBoolean(reader["IsCompleted"]),
            CompletedAt = reader["CompletedAt"] == DBNull.Value
                ? null
                : Convert.ToDateTime(reader["CompletedAt"])
        });
    }

    return slides;
}

public async Task<object?> GetRecommendedCourses(string studentId, int topN)
{
    using var conn = GetConnection();

    using var cmd = new SqlCommand("LMS.SP_GetRecommendedCoursesForStudent", conn);

    cmd.CommandType = CommandType.StoredProcedure;

    cmd.Parameters.AddWithValue("@StudentId", studentId);
    cmd.Parameters.AddWithValue("@TopN", topN);

    await conn.OpenAsync();

    using var reader = await cmd.ExecuteReaderAsync();

    var recommendations = new List<object>();

    while (await reader.ReadAsync())
    {
        recommendations.Add(new
        {
            CourseId = reader["CourseId"],
            CourseTitle = reader["CourseTitle"],
            CourseDescription = reader["CourseDescription"] as string,
            CourseIconUrl = reader["CourseIconUrl"] as string,
            Tags = reader["Tags"] as string,
            CompletionTimeSeconds = Convert.ToInt32(reader["CompletionTimeSeconds"]),
            InterestScore = Convert.ToInt32(reader["InterestScore"]),
            HistoryScore = Convert.ToInt32(reader["HistoryScore"]),
            MatchScore = Convert.ToInt32(reader["MatchScore"])
        });
    }

    return recommendations;
}

public async Task<object?> GetRecommendedAssignments(string studentId, int topN)
{
    using var conn = GetConnection();

    using var cmd = new SqlCommand("LMS.SP_GetRecommendedAssignmentsForStudent", conn);

    cmd.CommandType = CommandType.StoredProcedure;

    cmd.Parameters.AddWithValue("@StudentId", studentId);
    cmd.Parameters.AddWithValue("@TopN", topN);

    await conn.OpenAsync();

    using var reader = await cmd.ExecuteReaderAsync();

    var recommendations = new List<object>();

    while (await reader.ReadAsync())
    {
        recommendations.Add(new
        {
            AssignmentId = reader["AssignmentId"],
            AssignmentTitle = reader["AssignmentTitle"],
            AssignmentDescription = reader["AssignmentDescription"] as string,
            AssessmentIconUrl = reader["AssessmentIconUrl"] as string,
            Tags = reader["Tags"] as string,
            CompletionTimeSeconds = Convert.ToInt32(reader["CompletionTimeSeconds"]),
            PassPercentage = Convert.ToDecimal(reader["PassPercentage"]),
            InterestScore = Convert.ToInt32(reader["InterestScore"]),
            HistoryScore = Convert.ToInt32(reader["HistoryScore"]),
            MatchScore = Convert.ToInt32(reader["MatchScore"])
        });
    }

    return recommendations;
}

public async Task<int> SaveStudentAnswers(
    string studentId, int assignmentId,
    List<(int QuestionId, string SelectedOption, bool IsCorrect)> answers)
{
    using var conn = GetConnection();

    using var cmd = new SqlCommand("LMS.SP_SaveStudentAnswers", conn);

    cmd.CommandType = CommandType.StoredProcedure;

    var answersJson = JsonSerializer.Serialize(answers.Select(a => new
    {
        a.QuestionId,
        a.SelectedOption,
        a.IsCorrect
    }));

    cmd.Parameters.AddWithValue("@StudentId", studentId);
    cmd.Parameters.AddWithValue("@AssignmentId", assignmentId);
    cmd.Parameters.AddWithValue("@AnswersJson", answersJson);

    await conn.OpenAsync();

    var result = await cmd.ExecuteScalarAsync();

    return result == null ? 1 : Convert.ToInt32(result);
}

}
