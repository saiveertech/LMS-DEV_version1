CREATE PROCEDURE LMS.SP_TerminateSession
(
    @SessionId NVARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (
        SELECT 1 FROM LMS.UserSessions
        WHERE SessionId = @SessionId AND IsActive = 1
    )
    BEGIN
        RAISERROR('Session not found or already terminated.', 16, 1);
        RETURN;
    END

    UPDATE LMS.UserSessions
    SET IsActive    = 0,
        LogoutTime  = SYSUTCDATETIME()
    WHERE SessionId = @SessionId;

END
GO
