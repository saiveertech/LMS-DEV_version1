CREATE PROCEDURE LMS.SP_RegisterStudent
(
@FirstName NVARCHAR(100),
@LastName NVARCHAR(100),
@Email NVARCHAR(200),
@PhoneNumber NVARCHAR(50),
@Password NVARCHAR(MAX),
@EducationDetails NVARCHAR(MAX),
@AreaOfInterest NVARCHAR(MAX),
@StudentId NVARCHAR(50) OUTPUT
)
AS
BEGIN
DECLARE @Placeholder NVARCHAR(50) = CONVERT(NVARCHAR(50), NEWID());
DECLARE @NewId INT;

INSERT INTO LMS.Students
(
StudentId,
FirstName,
LastName,
Email,
PhoneNumber,
Password,
EducationDetails,
AreaOfInterest
)
VALUES
(
@Placeholder,
@FirstName,
@LastName,
@Email,
@PhoneNumber,
@Password,
@EducationDetails,
@AreaOfInterest
)

SET @NewId = CAST(SCOPE_IDENTITY() AS INT);

SET @StudentId =
    'SK' +
    UPPER(LEFT(@FirstName, 1)) +
    UPPER(LEFT(@LastName, 1)) +
    RIGHT('000' + CAST(@NewId AS VARCHAR(10)), 3) +
    'SD';

UPDATE LMS.Students
SET StudentId = @StudentId
WHERE StudentId = @Placeholder;
END
GO
