using LMS.Infrastructure.Repositories.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DatabaseController : ControllerBase
{
    private readonly AuthRepository _repo;

    public DatabaseController(AuthRepository repo)
    {
        _repo = repo;
    }

    [HttpGet("test")]
    public IActionResult Test()
    {
        try
        {
            using var conn = _repo.GetConnection();
            conn.Open();

            return Ok(new
            {
                Success = true,
                Message = "Database connected successfully"
            });
        }
        catch (Exception)
        {
            return StatusCode(500, new
            {
                Success = false,
                Message = "Database connection failed"
            });
        }
    }
}
