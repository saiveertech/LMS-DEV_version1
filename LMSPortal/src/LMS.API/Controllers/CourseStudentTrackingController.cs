using LMS.Application.Common;
using LMS.Application.Features.CourseStudentTracking.DTOs;
using LMS.Application.Features.CourseStudentTracking.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers;

[ApiController]
[Route("api/course")]
[Authorize(Roles = Roles.Admin + "," + Roles.Trainer)]
[Tags("CourseStudentTracking")]
public class CourseStudentTrackingController : ControllerBase
{
    private readonly ICourseStudentTrackingService _service;

    public CourseStudentTrackingController(ICourseStudentTrackingService service)
    {
        _service = service;
    }

    // ─── Get Course Students ─────────────────────────────────────────────────

    [HttpGet("{courseId:int}/students")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetCourseStudents(
        int courseId,
        [FromQuery] string? status,
        [FromQuery] string? studentId,
        [FromQuery] bool? certificateGenerated)
    {
        if (courseId <= 0)
        {
            return BadRequest(new ServiceResponse
            {
                Success = false,
                Message = "A valid Course ID is required."
            });
        }

        var filter = new CourseStudentTrackingFilter
        {
            Status = status,
            StudentId = studentId,
            CertificateGenerated = certificateGenerated
        };

        var students = await _service.GetCourseStudents(courseId, filter);

        return Ok(new ServiceResponse
        {
            Success = true,
            Message = students.Count > 0
                ? $"Found {students.Count} student(s) enrolled in this course."
                : "No students found for this course.",
            Data = students
        });
    }

    // ─── Update Course Student Progress ──────────────────────────────────────
    // CourseStatus/RegistrationStatus are derived from CompletedLessons vs
    // TotalLessons, never set directly — reaching 100% auto-marks the
    // enrollment Completed.

    [HttpPut("{courseId:int}/students/{studentId}/progress")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateCourseStudentProgress(
        int courseId,
        string studentId,
        [FromBody] UpdateCourseProgressRequest request)
    {
        var result = await _service.UpdateCourseStudentProgress(courseId, studentId, request);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    // ─── Get Assignment Students ─────────────────────────────────────────────
    // Mirrors GetCourseStudents but for assignment-side reporting. Route is
    // overridden (~/) since this controller's base route is "api/course".

    [HttpGet("~/api/assignment/{assignmentId:int}/students")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAssignmentStudents(
        int assignmentId,
        [FromQuery] string? status,
        [FromQuery] string? studentId,
        [FromQuery] bool? certificateGenerated)
    {
        if (assignmentId <= 0)
        {
            return BadRequest(new ServiceResponse
            {
                Success = false,
                Message = "A valid Assignment ID is required."
            });
        }

        var filter = new CourseStudentTrackingFilter
        {
            Status = status,
            StudentId = studentId,
            CertificateGenerated = certificateGenerated
        };

        var students = await _service.GetAssignmentStudents(assignmentId, filter);

        return Ok(new ServiceResponse
        {
            Success = true,
            Message = students.Count > 0
                ? $"Found {students.Count} student(s) enrolled in this assignment."
                : "No students found for this assignment.",
            Data = students
        });
    }
}
