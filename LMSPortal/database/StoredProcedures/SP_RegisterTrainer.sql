CREATE PROCEDURE LMS.SP_RegisterTrainer
(
@FirstName NVARCHAR(100),
@LastName NVARCHAR(100),
@Email NVARCHAR(200),
@PhoneNumber NVARCHAR(50),
@Password NVARCHAR(MAX),
@ExperienceYears INT,
@CurrentCompany NVARCHAR(200),
@Designation NVARCHAR(200),
@Bio NVARCHAR(MAX),
@LinkedInUrl NVARCHAR(500),
@TrainerId NVARCHAR(50) OUTPUT
)
AS
BEGIN
DECLARE @Placeholder NVARCHAR(50) = CONVERT(NVARCHAR(50), NEWID());
DECLARE @NewId INT;

INSERT INTO LMS.Trainers
(
TrainerId,
FirstName,
LastName,
Email,
PhoneNumber,
Password,
ExperienceYears,
CurrentCompany,
Designation,
Bio,
LinkedInUrl
)
VALUES
(
@Placeholder,
@FirstName,
@LastName,
@Email,
@PhoneNumber,
@Password,
@ExperienceYears,
@CurrentCompany,
@Designation,
@Bio,
@LinkedInUrl
)

SET @NewId = CAST(SCOPE_IDENTITY() AS INT);

SET @TrainerId =
    'SK' +
    UPPER(LEFT(@FirstName, 1)) +
    UPPER(LEFT(@LastName, 1)) +
    RIGHT('000' + CAST(@NewId AS VARCHAR(10)), 3) +
    'TR';

UPDATE LMS.Trainers
SET TrainerId = @TrainerId
WHERE TrainerId = @Placeholder;
END
GO
