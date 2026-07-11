using LMS.Application.Common;
using LMS.Application.Features.Auth.DTOs;
using LMS.Application.Features.Session.Services;
using LMS.Infrastructure.Repositories.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthRepository _repo;
    private readonly IConfiguration _config;
    private readonly ISessionService _sessionService;

    public AuthController(AuthRepository repo, IConfiguration config, ISessionService sessionService)
    {
        _repo = repo;
        _config = config;
        _sessionService = sessionService;
    }

    // Public — generates JWT token after verifying email + password
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        try
        {
            var result = await _repo.Login(request.Email, request.Password);

            if (string.IsNullOrEmpty(result.Email))
                return Unauthorized(new
                {
                    Success = false,
                    Message = "Invalid email or password"
                });

            // Single-device login: blocked if this user already has an active session
            var deviceInfo = Request.Headers.UserAgent.ToString();

            var session = await _sessionService.CreateSessionAsync(result.Id, result.Role, deviceInfo);

            if (!session.Success)
                return Conflict(new
                {
                    Success = false,
                    Message = session.Message
                });

            var token = GenerateJwtToken(result.Email, result.Role, result.Id, result.Name, session.SessionId!);

            return Ok(new
            {
                Success = true,
                Message = "Login successful",
                Token = token.Value,
                Id = result.Id,
                Role = result.Role,
                Email = result.Email,
                ExpiresAt = token.ExpiresAt
            });
        }
        catch (Exception)
        {
            return StatusCode(500, new
            {
                Success = false,
                Message = "An error occurred during login"
            });
        }
    }

    // Public — sets a new password for the given email. Only succeeds if
    // that email was verified via verify-otp within the last 15 minutes.
    // No OTP or old password required here.
    [AllowAnonymous]
[HttpPost("reset-password")]
public async Task<IActionResult> ResetPassword(
    ResetPasswordRequest request)
{
    var (success, message) = await _repo.ResetPasswordAfterVerification(
        request.Email,
        request.NewPassword);

    if (!success)
    {
        return BadRequest(new
        {
            Success = false,
            Message = message
        });
    }

    return Ok(new
    {
        Success = true,
        Message = message
    });
}

    // Public — accepts a registered email and, if found among
    // Students/Trainers/Admin, emails a one-time OTP for password reset.
    // Always returns a generic success message to avoid leaking whether
    // an email is registered.
    [AllowAnonymous]
    [HttpPost("send-otp")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
    {
        try
        {
            await _repo.GenerateAndSendOtp(request.Email);

            return Ok(new
            {
                Success = true,
                Message = "If an account with that email exists, an OTP has been sent."
            });
        }
        catch (Exception)
        {
            return StatusCode(500, new
            {
                Success = false,
                Message = "An error occurred while processing your request"
            });
        }
    }

    // Public — verifies the OTP sent via forgot-password. On success,
    // marks the email verified for 15 minutes, during which reset-password
    // can be called with just { Email, NewPassword }.
    [AllowAnonymous]
    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp(VerifyOtpRequest request)
    {
        try
        {
            var (success, message) = await _repo.VerifyOtp(
                request.Email,
                request.Otp);

            if (!success)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = message
                });
            }

            return Ok(new
            {
                Success = true,
                Message = message
            });
        }
        catch (Exception)
        {
            return StatusCode(500, new
            {
                Success = false,
                Message = "An error occurred while verifying the OTP"
            });
        }
    }

    private (string Value, DateTime ExpiresAt) GenerateJwtToken(
        string email,
        string role,
        string userId,
        string fullName,
        string sessionId)
    {
        var secretKey = _config["JwtSettings:SecretKey"]!;
        var issuer    = _config["JwtSettings:Issuer"]!;
        var audience  = _config["JwtSettings:Audience"]!;
        var expiryMin = int.Parse(_config["JwtSettings:ExpiryMinutes"]!);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, email),
            new Claim(ClaimTypes.Role, role),
            new Claim(AppClaimTypes.UserId, userId),
            new Claim(AppClaimTypes.FullName, fullName),
            new Claim(AppClaimTypes.SessionId, sessionId)
        };

        var expiry = DateTime.UtcNow.AddMinutes(expiryMin);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiry,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiry);
    }
}
