using LMS.Application.Features.Admin.DTOs;
using LMS.Application.Features.Auth.Services.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Tags("Admin")]
public class AdminController : ControllerBase
{
    private readonly AdminService _service;

    public AdminController(AdminService service)
    {
        _service = service;
    }

    [HttpPost("register-admin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterAdmin(RegisterAdminRequest request)
    {
        var result = await _service.RegisterAdmin(request);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpGet("get-admin-details/{adminId?}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAdmin(string? adminId)
    {
        var result = await _service.GetAdminById(adminId);

        if (adminId != null && result == null)
        {
            return NotFound(new { Message = "Admin Not Found" });
        }

        return Ok(result);
    }

    /// <summary>
/// Update Admin
/// </summary>
[HttpPut("update-admin/{adminId}")]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
public async Task<IActionResult> UpdateAdmin(
    string adminId,
    UpdateAdminRequest request)
{
    var result =
        await _service.UpdateAdmin(
            adminId,
            request);

    if (!result.Success)
    {
        if(result.Message=="Admin Not Found.")
            return NotFound(result);

        return BadRequest(result);
    }

    return Ok(result);
}
}