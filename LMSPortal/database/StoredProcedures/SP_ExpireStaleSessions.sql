CREATE PROCEDURE LMS.SP_ExpireStaleSessions
(
    @IdleTimeoutMinutes INT,
    @ExpiryMinutes      INT
)
AS
BEGIN
    SET NOCOUNT ON;

    -- Background sweep: catches sessions nobody's request will ever touch
    -- again (e.g. laptop closed, app crashed) so UserSessions.IsActive stays
    -- accurate even without a login attempt or another request from the
    -- affected user to trigger cleanup.
    UPDATE LMS.UserSessions
    SET IsActive   = 0,
        LogoutTime = SYSUTCDATETIME()
    WHERE IsActive = 1
      AND (
            DATEADD(MINUTE, @IdleTimeoutMinutes, LastActivityTime) <= SYSUTCDATETIME()
         OR DATEADD(MINUTE, @ExpiryMinutes, LoginTime) <= SYSUTCDATETIME()
          );

    SELECT @@ROWCOUNT AS ExpiredCount;

END
GO
