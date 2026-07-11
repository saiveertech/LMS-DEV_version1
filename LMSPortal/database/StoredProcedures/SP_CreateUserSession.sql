CREATE PROCEDURE LMS.SP_CreateUserSession
(
    @SessionId      NVARCHAR(50),
    @UserId         NVARCHAR(50),
    @Role           NVARCHAR(50),
    @DeviceInfo     NVARCHAR(500),
    @ExpiryMinutes  INT
)
AS
BEGIN
    SET NOCOUNT ON;

    -- A session whose JWT would already be expired can never be terminated
    -- by its owner (the expired token fails auth before reaching /terminate),
    -- so it must not be allowed to block logins forever. Auto-expire it here.
    UPDATE LMS.UserSessions
    SET IsActive   = 0,
        LogoutTime = SYSUTCDATETIME()
    WHERE UserId    = @UserId
      AND IsActive  = 1
      AND DATEADD(MINUTE, @ExpiryMinutes, LoginTime) <= SYSUTCDATETIME();

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
        DeviceInfo
    )
    VALUES
    (
        @SessionId,
        @UserId,
        @Role,
        @DeviceInfo
    );

END
GO
