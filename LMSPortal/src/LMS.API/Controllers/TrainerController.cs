using LMS.Application.Features.Auth.DTOs;
using LMS.Infrastructure.Repositories.Trainer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TrainerController : ControllerBase
{
    private readonly TrainerRepository _repo;

    public TrainerController(TrainerRepository repo)
    {
        _repo = repo;
    }

    [HttpPost("register-trainer")]
    public async Task<IActionResult> RegisterTrainer(
        RegisterTrainerRequest request)
    {
        await _repo.RegisterTrainer(request);

        return Ok(new
        {
            Success = true,
            Message = "Trainer Registered Successfully"
        });
    }

    [HttpGet("get-trainer-details/{trainerId}")]
    public async Task<IActionResult> GetTrainerDetails(
        string trainerId)
    {
        var result =
            await _repo.GetTrainerById(trainerId);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

   [HttpPut("update-trainer-details/{trainerId}")]
public async Task<IActionResult> UpdateTrainerDetails(
    string trainerId,
    [FromBody] UpdateTrainerRequest request)
{
    var result = await _repo.UpdateTrainer(trainerId, request);

    if (!result)
        return NotFound();

    return Ok(new
    {
        Success = true,
        Message = "Trainer Updated Successfully"
    });
}
}