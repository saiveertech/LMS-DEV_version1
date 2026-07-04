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
        var createdBy = User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
        var createdByRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        var result = await _service.RegisterCourse(
            request,
            createdBy,
            createdByRole);

        if (!result.Success)
            return BadRequest(result);

        return StatusCode(
            StatusCodes.Status201Created,
            result);
    }

    [HttpGet("get-course-details/{courseId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCourseDetails(int courseId)
    {
        var result = await _service.GetCourseById(courseId);

        if (result == null)
        {
            return NotFound(new
            {
                Success = false,
                Message = "Course Not Found."
            });
        }

        return Ok(result);
    }
}
