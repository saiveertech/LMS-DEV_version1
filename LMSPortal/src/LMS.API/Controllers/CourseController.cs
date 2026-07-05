using System.Security.Claims;
using LMS.Application.Common;
using LMS.Application.Features.Course.DTOs;
using LMS.Application.Features.Course.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.Admin + "," + Roles.Trainer)]
[Tags("Course")]
public class CourseController : ControllerBase
{
    private readonly ICourseService _service;

    public CourseController(ICourseService service)
    {
        _service = service;
    }

    [HttpPost("register-course")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(524_288_000)]
    [RequestFormLimits(MultipartBodyLengthLimit = 524_288_000)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterCourse(
        [FromForm] CreateCourseRequest request)
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

        var result = await _service.RegisterCourse(
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

    [HttpGet("get-course-details/{courseId:int?}")]
    [ProducesResponseType(typeof(List<CourseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCourseDetails(int? courseId)
    {
        var result = await _service.GetCourseById(courseId);

        if (courseId.HasValue && result == null)
        {
            return NotFound(new
            {
                Success = false,
                Message = "Course Not Found."
            });
        }

        return Ok(result);
    }

    [HttpPut("update-course-details/{courseId:int}")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(524_288_000)]
    [RequestFormLimits(MultipartBodyLengthLimit = 524_288_000)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCourseDetails(
        int courseId,
        [FromForm] UpdateCourseRequest request)
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

        var result = await _service.UpdateCourse(
            courseId,
            request,
            editedById,
            editedByName,
            editedByRole);

        if (!result.Success)
        {
            if (result.Message == "Course Not Found.")
                return NotFound(result);

            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpDelete("{courseId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCourse(int courseId)
    {
        var deletedById = User.FindFirstValue(AppClaimTypes.UserId) ?? string.Empty;
        var deletedByName = User.FindFirstValue(AppClaimTypes.FullName) ?? string.Empty;
        var deletedByRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        var result = await _service.DeleteCourse(
            courseId,
            deletedById,
            deletedByName,
            deletedByRole);

        if (!result.Success)
        {
            if (result.Message == "Course Not Found.")
                return NotFound(result);

            return BadRequest(result);
        }

        return Ok(result);
    }
}
