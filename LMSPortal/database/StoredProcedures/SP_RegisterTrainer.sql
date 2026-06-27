CREATE PROCEDURE LMS.SP_RegisterTrainer
(
@TrainerId NVARCHAR(50),
@FirstName NVARCHAR(100),
@LastName NVARCHAR(100),
@Email NVARCHAR(200),
@PhoneNumber NVARCHAR(50),
@Password NVARCHAR(MAX),
@ExperienceYears INT,
@CurrentCompany NVARCHAR(200),
@Designation NVARCHAR(200),
@Bio NVARCHAR(MAX),
@LinkedInUrl NVARCHAR(500)
)
AS
BEGIN
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
@TrainerId,
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
END
GO
