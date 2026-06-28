using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace LMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DatabaseController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public DatabaseController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet("test")]
    public IActionResult Test()
    {
        try
        {
            using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            conn.Open();

            return Ok(new
            {
                Success = true,
                Message = "Database connected successfully"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                Success = false,
                Message = ex.Message,
                Details = ex.ToString()
            });
        }
    }
}