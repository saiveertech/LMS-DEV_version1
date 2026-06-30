using LMS.Application.Features.Auth.DTOs;
using LMS.Application.Features.Auth.Services.Trainer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Tags("Trainer")]
public class TrainerController : ControllerBase
{
    private readonly ITrainerService _service;

    public TrainerController(ITrainerService service)
    {
        _service = service;
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
    [HttpGet("get-trainer-details/{trainerId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTrainerDetails(
        string trainerId)
    {
        var result = await _service.GetTrainerById(trainerId);

        if (result == null)
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
}