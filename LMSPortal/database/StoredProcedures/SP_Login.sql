CREATE PROCEDURE LMS.SP_Login
(
@Email NVARCHAR(200)
)
AS
BEGIN


IF EXISTS
(
    SELECT 1
    FROM LMS.Students
    WHERE Email = @Email
)
BEGIN
    SELECT
        StudentId,
        Email,
        Password,
        'Student' AS Role
    FROM LMS.Students
    WHERE Email = @Email

    RETURN
END

IF EXISTS
(
    SELECT 1
    FROM LMS.Trainers
    WHERE Email = @Email
)
BEGIN
    SELECT
        TrainerId,
        Email,
        Password,
        'Trainer' AS Role
    FROM LMS.Trainers
    WHERE Email = @Email
END


END
GO
