CREATE PROCEDURE LMS.SP_CreateUserSession
(
    @SessionId          NVARCHAR(50),
    @UserId             NVARCHAR(50),
    @Role               NVARCHAR(50),
    @DeviceInfo         NVARCHAR(500),
    @ExpiryMinutes      INT,
    @IdleTimeoutMinutes INT
)
AS
BEGIN
    SET NOCOUNT ON;

    -- A session that's gone idle (no heartbeat/request) or whose JWT would
    -- already be expired can never be terminated by its owner (a dead
    -- browser can't call /terminate), so it must not block logins forever.
    -- Auto-expire on whichever threshold is crossed first.
    UPDATE LMS.UserSessions
    SET IsActive   = 0,
        LogoutTime = SYSUTCDATETIME()
    WHERE UserId    = @UserId
      AND IsActive  = 1
      AND (
            DATEADD(MINUTE, @IdleTimeoutMinutes, LastActivityTime) <= SYSUTCDATETIME()
         OR DATEADD(MINUTE, @ExpiryMinutes, LoginTime) <= SYSUTCDATETIME()
          );

    -- Single-device login: block if this user already has an active session
    IF EXISTS (
        SELECT 1 FROM LMS.UserSessions
        WHERE UserId = @UserId AND IsActive = 1
    )
    BEGIN
        RAISERROR('An active session already exists for this user.', 16, 1);
        RETURN;
    END

    INSERT INTO LMS.UserSessions
    (
        SessionId,
        UserId,
        Role,
        DeviceInfo,
        LastActivityTime
    )
    VALUES
    (
        @SessionId,
        @UserId,
        @Role,
        @DeviceInfo,
        SYSUTCDATETIME()
    );

END
GO
