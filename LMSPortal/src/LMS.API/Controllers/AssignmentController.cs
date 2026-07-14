using System.Security.Claims;
using LMS.Application.Common;
using LMS.Application.Features.Assignment.DTOs;
using LMS.Application.Features.Assignment.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.Admin + "," + Roles.Trainer)]
[Tags("Assignment")]
public class AssignmentController : ControllerBase
{
    private readonly IAssignmentService _service;

    public AssignmentController(IAssignmentService service)
    {
        _service = service;
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(524_288_000)]
    [RequestFormLimits(MultipartBodyLengthLimit = 524_288_000)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAssignment(
        [FromForm] CreateAssignmentRequest request)
    {
        var createdById = !string.IsNullOrWhiteSpace(request.CreatedById)
            ? request.CreatedById
            : User.FindFirstValue(AppClaimTypes.UserId) ?? string.Empty;

        var createdByName = !string.IsNullOrWhiteSpace(request.CreatedByName)
            ? request.CreatedByName
            : User.FindFirstValue(AppClaimTypes.FullName) ?? string.Empty;

        var createdByRole = !string.IsNullOrWhiteSpace(request.CreatedByRole)
            ? request.CreatedByRole
            : User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        var result = await _service.CreateAssignment(
            request,
            createdById,
            createdByName,
            createdByRole);

        if (!result.Success)
            return BadRequest(result);

        return StatusCode(
            StatusCodes.Status201Created,
            result);
    }

    [HttpGet("{id:int?}")]
    [ProducesResponseType(typeof(List<AssignmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAssignments(int? id)
    {
        var result = await _service.GetAssignments(id);

        if (id.HasValue && result == null)
        {
            return NotFound(new
            {
                Success = false,
                Message = "Assignment Not Found."
            });
        }

        return Ok(result);
    }

    [HttpPut("{id:int}")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(524_288_000)]
    [RequestFormLimits(MultipartBodyLengthLimit = 524_288_000)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAssignment(
        int id,
        [FromForm] UpdateAssignmentRequest request)
    {
        var editedById = !string.IsNullOrWhiteSpace(request.EditedById)
            ? request.EditedById
            : User.FindFirstValue(AppClaimTypes.UserId) ?? string.Empty;

        var editedByName = !string.IsNullOrWhiteSpace(request.EditedByName)
            ? request.EditedByName
            : User.FindFirstValue(AppClaimTypes.FullName) ?? string.Empty;

        var editedByRole = !string.IsNullOrWhiteSpace(request.EditedByRole)
            ? request.EditedByRole
            : User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        var result = await _service.UpdateAssignment(
            id,
            request,
            editedById,
            editedByName,
            editedByRole);

        if (!result.Success)
        {
            if (result.Message == "Assignment Not Found.")
                return NotFound(result);

            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAssignment(int id)
    {
        var deletedById = User.FindFirstValue(AppClaimTypes.UserId) ?? string.Empty;
        var deletedByName = User.FindFirstValue(AppClaimTypes.FullName) ?? string.Empty;
        var deletedByRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        var result = await _service.DeleteAssignment(
            id,
            deletedById,
            deletedByName,
            deletedByRole);

        if (!result.Success)
        {
            if (result.Message == "Assignment Not Found.")
                return NotFound(result);

            return BadRequest(result);
        }

        return Ok(result);
    }

    // ─── Get Questions (trainer/admin — includes correct answers) ───────────

    [HttpGet("{assignmentId:int}/questions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetQuestions(int assignmentId)
    {
        var questions = await _service.GetQuestions(assignmentId);

        return Ok(new
        {
            Success = true,
            Message = questions.Count > 0
                ? $"Found {questions.Count} question(s)."
                : "No questions found — has the answer-key Excel file been uploaded?",
            Data = questions
        });
    }
}
