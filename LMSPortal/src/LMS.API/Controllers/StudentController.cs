using LMS.Application.Features.Auth.DTOs;
using LMS.Infrastructure.Repositories.Student;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StudentController : ControllerBase
{
    private readonly StudentRepository _repo;

    public StudentController(StudentRepository repo)
    {
        _repo = repo;
    }

    [HttpPost("register-student")]
    public async Task<IActionResult> RegisterStudent(
        RegisterStudentRequest request)
    {
        await _repo.RegisterStudent(request);

        return Ok(new
        {
            Success = true,
            Message = "Student Registered Successfully"
        });
    }

    [HttpGet("get-student-details/{studentId}")]
    public async Task<IActionResult> GetStudentDetails(
        string studentId)
    {
        var result =
            await _repo.GetStudentById(studentId);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpPut("update-student-details/{studentId}")]
    public async Task<IActionResult> UpdateStudentDetails(
        string studentId,
        RegisterStudentRequest request)
    {
        var result =
            await _repo.UpdateStudent(
                studentId,
                request);

        if (!result)
            return NotFound();

        return Ok(new
        {
            Success = true,
            Message = "Student Updated Successfully"
        });
    }
}