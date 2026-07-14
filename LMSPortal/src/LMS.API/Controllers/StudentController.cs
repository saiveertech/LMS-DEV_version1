using LMS.Application.Features.Assignment.Services;
using LMS.Application.Features.Auth.DTOs;
using LMS.Application.Features.Auth.Services.Student;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Tags("Student")]
public class StudentController : ControllerBase
{
    private readonly IStudentService _service;
    private readonly IAssignmentService _assignmentService;

    public StudentController(IStudentService service, IAssignmentService assignmentService)
    {
        _service = service;
        _assignmentService = assignmentService;
    }

    [HttpPost("register-student")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterStudent(
        RegisterStudentRequest request)
    {
        var result = await _service.RegisterStudent(request);

        if (!result.Success)
            return BadRequest(result);

        return StatusCode(
            StatusCodes.Status201Created,
            result);
    }

    [HttpGet("get-student-details/{studentId?}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStudentDetails(
        string? studentId)
    {
        var result =
            await _service.GetStudentById(studentId);

        if (studentId != null && result == null)
        {
            return NotFound(new
            {
                Success = false,
                Message = "Student Not Found."
            });
        }

        return Ok(result);
    }

    [HttpPut("update-student-details/{studentId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStudentDetails(
        string studentId,
        UpdateStudentRequest request)
    {
        var result =
            await _service.UpdateStudent(
                studentId,
                request);

        if (!result.Success)
        {
            if (result.Message == "Student Not Found.")
                return NotFound(result);

            return BadRequest(result);
        }

        return Ok(result);
    }

    // ─── Course Enrollment ───────────────────────────────────────────────────

    [HttpPost("enroll-course")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EnrollCourse(
        [FromBody] EnrollCourseRequest request)
    {
        var result = await _service.EnrollCourse(request);

        if (!result.Success)
            return BadRequest(result);

        return StatusCode(
            StatusCodes.Status201Created,
            result);
    }

    [HttpGet("enrolled-courses/{studentId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEnrolledCourses(string studentId)
    {
        if (string.IsNullOrWhiteSpace(studentId))
        {
            return BadRequest(new
            {
                Success = false,
                Message = "Student ID is required."
            });
        }

        var result = await _service.GetEnrolledCourses(studentId);

        return Ok(new
        {
            Success = true,
            StudentId = studentId,
            Data = result
        });
    }

    // ─── Assignment Enrollment ───────────────────────────────────────────────

    [HttpPost("enroll-assignment")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EnrollAssignment(
        [FromBody] EnrollAssignmentRequest request)
    {
        var result = await _service.EnrollAssignment(request);

        if (!result.Success)
            return BadRequest(result);

        return StatusCode(
            StatusCodes.Status201Created,
            result);
    }

    [HttpGet("enrolled-assignments/{studentId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEnrolledAssignments(string studentId)
    {
        if (string.IsNullOrWhiteSpace(studentId))
        {
            return BadRequest(new
            {
                Success = false,
                Message = "Student ID is required."
            });
        }

        var result = await _service.GetEnrolledAssignments(studentId);

        return Ok(new
        {
            Success = true,
            StudentId = studentId,
            Data = result
        });
    }

    // ─── Submit Assignment Attempt ───────────────────────────────────────────
    // AssessmentScore keeps the best score across attempts; AssignmentStatus
    // auto-flips to Completed once a passing score is reached and never
    // downgrades on a later worse attempt.

    [HttpPost("submit-assignment-score")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SubmitAssignmentAttempt(
        [FromBody] SubmitAssignmentAttemptRequest request)
    {
        var result = await _service.SubmitAssignmentAttempt(request);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    // ─── Complete Course Slide ────────────────────────────────────────────────
    // Recomputes CompletedLessons/TotalLessons/CourseStatus from actual slide
    // completions — the real "student progresses through the course" path.

    [HttpPost("complete-slide")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CompleteCourseSlide(
        [FromQuery] string studentId,
        [FromQuery] int slideId)
    {
        var result = await _service.CompleteCourseSlide(studentId, slideId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    // ─── Get Course Resume Point ──────────────────────────────────────────────
    // Returns every slide with IsCompleted flagged; the frontend's resume
    // point is the first slide where IsCompleted = false.

    [HttpGet("course-resume/{courseId:int}/{studentId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCourseResume(int courseId, string studentId)
    {
        var slides = await _service.GetCourseResume(studentId, courseId);

        return Ok(new
        {
            Success = true,
            StudentId = studentId,
            CourseId = courseId,
            Data = slides
        });
    }

    // ─── Get Assignment Questions (take the quiz) ────────────────────────────
    // Safe view — CorrectOption is never included here.

    [HttpGet("assignment-questions/{assignmentId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAssignmentQuestions(int assignmentId)
    {
        var questions = await _assignmentService.GetQuestionsForStudent(assignmentId);

        return Ok(new
        {
            Success = true,
            Message = questions.Count > 0
                ? $"Found {questions.Count} question(s)."
                : "No questions found for this assignment.",
            Data = questions
        });
    }

    // ─── Submit Assignment Answers ───────────────────────────────────────────
    // Real evaluation against the answer key — score is computed here, not
    // trusted from the caller. Feeds into the same attempt/pass/certificate
    // flow as submit-assignment-score.

    [HttpPost("submit-assignment-answers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SubmitAssignmentAnswers(
        [FromBody] SubmitAssignmentAnswersRequest request)
    {
        var result = await _service.SubmitAssignmentAnswers(request);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}