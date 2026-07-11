CREATE PROCEDURE LMS.SP_ValidateAndTouchSession
(
    @SessionId          NVARCHAR(50),
    @IdleTimeoutMinutes INT
)
AS
BEGIN
    SET NOCOUNT ON;

    -- Kill this session immediately if it has gone idle too long, so a
    -- stale token gets rejected on its very next use rather than waiting
    -- for the full JWT lifetime or a background sweep to catch it.
    UPDATE LMS.UserSessions
    SET IsActive   = 0,
        LogoutTime = SYSUTCDATETIME()
    WHERE SessionId = @SessionId
      AND IsActive  = 1
      AND DATEADD(MINUTE, @IdleTimeoutMinutes, LastActivityTime) <= SYSUTCDATETIME();

    -- Still active — this request/heartbeat counts as activity
    UPDATE LMS.UserSessions
    SET LastActivityTime = SYSUTCDATETIME()
    WHERE SessionId = @SessionId
      AND IsActive  = 1;

    SELECT
        CASE WHEN EXISTS (
            SELECT 1 FROM LMS.UserSessions
            WHERE SessionId = @SessionId AND IsActive = 1
        ) THEN 1 ELSE 0 END AS IsActive;

END
GO
