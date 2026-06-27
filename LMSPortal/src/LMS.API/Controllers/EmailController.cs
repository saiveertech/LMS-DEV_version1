using LMS.Infrastructure.Email;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmailController : ControllerBase
{
    private readonly EmailService _emailService;

    public EmailController(EmailService emailService)
    {
        _emailService = emailService;
    }

    [HttpPost("test-email")]
public async Task<IActionResult> TestEmail()
{
    await _emailService.SendEmailAsync(
        "bhupathireddykistaram@gmail.com",
        "Sai Veer",
        "SkillToRole LMS Test Email",
        @"
        <h2>Welcome to SkillToRole LMS</h2>
        <p>Email integration is working successfully.</p>
        <p>This email was sent using Brevo API.</p>
        ");

    return Ok(new
    {
        Success = true,
        Message = "Email Sent Successfully"
    });
}
}