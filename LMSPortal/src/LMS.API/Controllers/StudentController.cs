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

    public StudentController(IStudentService service)
    {
        _service = service;
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
}