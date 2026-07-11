CREATE PROCEDURE LMS.SP_GetSessionBySessionId
(
    @SessionId NVARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        Id,
        SessionId,
        UserId,
        Role,
        DeviceInfo,
        LoginTime,
        LogoutTime,
        IsActive
    FROM LMS.UserSessions
    WHERE SessionId = @SessionId;

END
GO
