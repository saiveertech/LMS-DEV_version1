using LMS.Application.Common;
using LMS.Application.Features.Session.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SessionController : ControllerBase
{
    private readonly ISessionService _service;

    public SessionController(ISessionService service)
    {
        _service = service;
    }

    // ─── Get Current Session ─────────────────────────────────────────────────

    [HttpGet("current")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCurrentSession()
    {
        var sessionId = User.FindFirstValue(AppClaimTypes.SessionId);

        if (string.IsNullOrWhiteSpace(sessionId))
        {
            return BadRequest(new ServiceResponse
            {
                Success = false,
                Message = "No session found on this token."
            });
        }

        var result = await _service.GetCurrentSessionAsync(sessionId);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    // ─── Terminate Session ───────────────────────────────────────────────────

    [HttpPost("terminate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> TerminateSession()
    {
        var sessionId = User.FindFirstValue(AppClaimTypes.SessionId);

        if (string.IsNullOrWhiteSpace(sessionId))
        {
            return BadRequest(new ServiceResponse
            {
                Success = false,
                Message = "No session found on this token."
            });
        }

        var result = await _service.TerminateSessionAsync(sessionId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    // ─── Heartbeat ────────────────────────────────────────────────────────────
    // Frontend should call this on an interval (shorter than SessionSettings:
    // IdleTimeoutMinutes) to keep a genuinely active session from being
    // treated as abandoned, e.g. while the user sits on a video/reading page
    // that doesn't otherwise call any API.

    [HttpPost("heartbeat")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Heartbeat()
    {
        var sessionId = User.FindFirstValue(AppClaimTypes.SessionId);

        if (string.IsNullOrWhiteSpace(sessionId))
        {
            return BadRequest(new ServiceResponse
            {
                Success = false,
                Message = "No session found on this token."
            });
        }

        var isActive = await _service.ValidateAndTouchSessionAsync(sessionId);

        if (!isActive)
        {
            return Unauthorized(new ServiceResponse
            {
                Success = false,
                Message = "Session is no longer active. Please log in again."
            });
        }

        return Ok(new ServiceResponse
        {
            Success = true,
            Message = "Heartbeat recorded."
        });
    }
}
