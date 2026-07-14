using LMS.Application.Common;
using LMS.Application.Features.Certificate.DTOs;
using LMS.Application.Features.Certificate.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CertificateController : ControllerBase
{
    private readonly ICertificateService _service;

    public CertificateController(ICertificateService service)
    {
        _service = service;
    }

    // ─── Get Student Certificates ────────────────────────────────────────────

    [HttpGet("student/{studentId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStudentCertificates(string studentId)
    {
        if (string.IsNullOrWhiteSpace(studentId))
        {
            return BadRequest(new ServiceResponse
            {
                Success = false,
                Message = "Student ID is required."
            });
        }

        try
        {
            var certificates = await _service.GetStudentCertificates(studentId);

            return Ok(new ServiceResponse
            {
                Success = true,
                Message = certificates.Count > 0
                    ? $"Found {certificates.Count} certificate(s)."
                    : "No certificates found for this student.",
                Data = certificates
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ServiceResponse
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    // ─── Download Certificate ────────────────────────────────────────────────

    [HttpGet("download/{certificateId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadCertificate(string certificateId)
    {
        if (string.IsNullOrWhiteSpace(certificateId))
        {
            return BadRequest(new ServiceResponse
            {
                Success = false,
                Message = "Certificate ID is required."
            });
        }

        try
        {
            var certificate = await _service.GetCertificateById(certificateId);

            if (certificate == null)
            {
                return NotFound(new ServiceResponse
                {
                    Success = false,
                    Message = "Certificate not found."
                });
            }

            if (string.IsNullOrWhiteSpace(certificate.CertificateUrl))
            {
                return NotFound(new ServiceResponse
                {
                    Success = false,
                    Message = "Certificate PDF not available."
                });
            }

            // Redirect to the Azure Blob SAS URL for download
            return Redirect(certificate.CertificateUrl);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ServiceResponse
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    // ─── Verify Certificate ──────────────────────────────────────────────────

    [HttpGet("verify/{certificateId}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VerifyCertificate(string certificateId)
    {
        if (string.IsNullOrWhiteSpace(certificateId))
        {
            return BadRequest(new ServiceResponse
            {
                Success = false,
                Message = "Certificate ID is required."
            });
        }

        var result = await _service.VerifyCertificate(certificateId);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }
}
