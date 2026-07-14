using LMS.Application.Common;
using LMS.Application.Features.Auth.DTOs;
using LMS.Application.Features.Auth.Services.Student;
using LMS.Application.Features.Auth.Services.Trainer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Tags("Trainer")]
public class TrainerController : ControllerBase
{
    private readonly ITrainerService _service;
    private readonly IStudentService _studentService;

    public TrainerController(ITrainerService service, IStudentService studentService)
    {
        _service = service;
        _studentService = studentService;
    }

    /// <summary>
    /// Register Trainer
    /// </summary>
    [HttpPost("register-trainer")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterTrainer(
        RegisterTrainerRequest request)
    {
        var result = await _service.RegisterTrainer(request);

        if (!result.Success)
            return BadRequest(result);

        return StatusCode(
            StatusCodes.Status201Created,
            result);
    }

    /// <summary>
    /// Get Trainer Details
    /// </summary>
    [HttpGet("get-trainer-details/{trainerId?}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTrainerDetails(
        string? trainerId)
    {
        var result = await _service.GetTrainerById(trainerId);

        if (trainerId != null && result == null)
        {
            return NotFound(new
            {
                Success = false,
                Message = "Trainer Not Found."
            });
        }

        return Ok(result);
    }

    /// <summary>
    /// Update Trainer
    /// </summary>
    [HttpPut("update-trainer-details/{trainerId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTrainer(
        string trainerId,
        UpdateTrainerRequest request)
    {
        var result = await _service.UpdateTrainer(
            trainerId,
            request);

        if (!result.Success)
        {
            if (result.Message == "Trainer Not Found.")
            {
                return NotFound(result);
            }

            return BadRequest(result);
        }

        return Ok(result);
    }

    // ─── Assign Course To Student ────────────────────────────────────────────
    // Distinct from the student's own self-registration (Student/enroll-course):
    // this creates the same enrollment row, plus an audit record of who
    // assigned it, so reporting can tell trainer-assigned apart from self-registered.

    [HttpPost("assign-course")]
    [Authorize(Roles = Roles.Admin + "," + Roles.Trainer)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AssignCourse([FromBody] EnrollCourseRequest request)
    {
        var assignedById = User.FindFirstValue(AppClaimTypes.UserId) ?? string.Empty;
        var assignedByName = User.FindFirstValue(AppClaimTypes.FullName) ?? string.Empty;
        var assignedByRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        var result = await _studentService.AssignCourseToStudent(
            request.StudentId, request.CourseId, assignedById, assignedByName, assignedByRole);

        if (!result.Success)
            return BadRequest(result);

        return StatusCode(StatusCodes.Status201Created, result);
    }

    // ─── Assign Assignment To Student ────────────────────────────────────────

    [HttpPost("assign-assignment")]
    [Authorize(Roles = Roles.Admin + "," + Roles.Trainer)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AssignAssignment([FromBody] EnrollAssignmentRequest request)
    {
        var assignedById = User.FindFirstValue(AppClaimTypes.UserId) ?? string.Empty;
        var assignedByName = User.FindFirstValue(AppClaimTypes.FullName) ?? string.Empty;
        var assignedByRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        var result = await _studentService.AssignAssignmentToStudent(
            request.StudentId, request.AssignmentId, assignedById, assignedByName, assignedByRole);

        if (!result.Success)
            return BadRequest(result);

        return StatusCode(StatusCodes.Status201Created, result);
    }
}