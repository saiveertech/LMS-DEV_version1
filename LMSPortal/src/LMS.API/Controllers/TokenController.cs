using LMS.Application.Common;
using LMS.Application.Features.Auth.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TokenController : ControllerBase
{
    private readonly IConfiguration _config;

    public TokenController(IConfiguration config)
    {
        _config = config;
    }

    // Public — generates a token from email + role (no password check, for bootstrap/testing)
    [AllowAnonymous]
[HttpPost("generate")]
public IActionResult GenerateToken(
    GenerateTokenRequest request)
{
    if (request.Email != "skilltorole@gmail.com" ||
        request.Password != "skill@123")
    {
        return Unauthorized(new
        {
            Success = false,
            Message = "Invalid Email or Password"
        });
    }

    var secretKey = _config["JwtSettings:SecretKey"]!;
    var issuer = _config["JwtSettings:Issuer"]!;
    var audience = _config["JwtSettings:Audience"]!;
    var expiryMin = int.Parse(
        _config["JwtSettings:ExpiryMinutes"]!);

    var key = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(secretKey));

    var credentials = new SigningCredentials(
        key,
        SecurityAlgorithms.HmacSha256);

    var claims = new[]
    {
        new Claim(ClaimTypes.Name,
            request.Email),

        new Claim(ClaimTypes.Role,
            "Admin"),

        new Claim(AppClaimTypes.UserId,
            "bootstrap-admin"),

        new Claim(AppClaimTypes.FullName,
            "Bootstrap Admin")
    };

    var expiry = DateTime.UtcNow
        .AddMinutes(expiryMin);

    var token = new JwtSecurityToken(
        issuer: issuer,
        audience: audience,
        claims: claims,
        expires: expiry,
        signingCredentials: credentials);

    return Ok(new
    {
        Success = true,
        Token = new JwtSecurityTokenHandler()
            .WriteToken(token),
        ExpiresAt = expiry
    });
}
    }

