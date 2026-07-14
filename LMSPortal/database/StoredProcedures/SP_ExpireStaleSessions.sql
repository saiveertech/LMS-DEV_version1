CREATE PROCEDURE LMS.SP_ExpireStaleSessions
(
    @IdleTimeoutMinutes INT,
    @ExpiryMinutes      INT
)
AS
BEGIN
    SET NOCOUNT ON;

    
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
