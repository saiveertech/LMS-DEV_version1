using LMS.Application.Features.Auth.DTOs;
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

    public AuthController(AuthRepository repo, IConfiguration config)
    {
        _repo = repo;
        _config = config;
    }

    // Public — generates JWT token after verifying email + password
    [Authorize]
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

            var token = GenerateJwtToken(result.Email, result.Role);

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

    [Authorize]
[HttpPost("reset-password")]
public async Task<IActionResult> ResetPassword(
    ResetPasswordRequest request)
{
    var result = await _repo.ResetPassword(
        request.Email,
        request.OldPassword,
        request.NewPassword);

    if (!result)
    {
        return BadRequest(new
        {
            Success = false,
            Message = "Invalid Email or Old Password"
        });
    }

    return Ok(new
    {
        Success = true,
        Message = "Password Updated Successfully"
    });
}
    
    private (string Value, DateTime ExpiresAt) GenerateJwtToken(string email, string role)
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
            new Claim(ClaimTypes.Role, role)
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
