CREATE PROCEDURE LMS.SP_ResetPassword
(
@Email NVARCHAR(200),
@Password NVARCHAR(MAX)
)
AS
BEGIN


UPDATE LMS.Students
SET Password = @Password
WHERE Email = @Email

UPDATE LMS.Trainers
SET Password = @Password
WHERE Email = @Email


END
GO
