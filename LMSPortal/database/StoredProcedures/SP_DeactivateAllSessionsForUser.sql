CREATE PROCEDURE LMS.SP_DeactivateAllSessionsForUser
(
    @UserId NVARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;

    -- Idempotent: no-op if the user has no active session. Used by flows
    -- that should never be blocked by the single-device gate (e.g. the
    -- bootstrap/testing token endpoint) — clears any prior session first
    -- so the subsequent SP_CreateUserSession call always succeeds.
    UPDATE LMS.UserSessions
    SET IsActive   = 0,
        LogoutTime = SYSUTCDATETIME()
    WHERE UserId   = @UserId
      AND IsActive = 1;

END
GO
