namespace LMS.Application.Features.Auth.DTOs;

// Used after a successful VerifyOtp call for that email — no OTP or old
// password required here, the prior verification is what authorizes this.
public class ResetPasswordRequest
{
    public string Email { get; set; } = string.Empty;

    public string NewPassword { get; set; } = string.Empty;
}
