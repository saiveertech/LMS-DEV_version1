-- Adds IsVerified to LMS.PasswordResetTokens to support the two-step
-- Forgot Password flow (Send OTP -> Verify OTP -> Reset Password using
-- just Email + NewPassword, authorized by the verified flag).
-- Safe to run once per database (guarded with IF NOT EXISTS checks).

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('LMS.PasswordResetTokens') AND name = 'IsVerified'
)
BEGIN
    ALTER TABLE LMS.PasswordResetTokens
    ADD IsVerified BIT NOT NULL CONSTRAINT DF_PasswordResetTokens_IsVerified DEFAULT 0;
END
GO
