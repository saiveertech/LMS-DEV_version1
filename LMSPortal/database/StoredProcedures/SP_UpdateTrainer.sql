CREATE PROCEDURE LMS.SP_UpdateTrainer
(
@TrainerId NVARCHAR(50),
@FirstName NVARCHAR(100),
@LastName NVARCHAR(100),
@Email NVARCHAR(200),
@PhoneNumber NVARCHAR(50),
@ExperienceYears INT,
@CurrentCompany NVARCHAR(200),
@Designation NVARCHAR(200),
@Bio NVARCHAR(MAX),
@LinkedInUrl NVARCHAR(500)
)
AS
BEGIN


UPDATE LMS.Trainers
SET
    FirstName = @FirstName,
    LastName = @LastName,
    Email = @Email,
    PhoneNumber = @PhoneNumber,
    ExperienceYears = @ExperienceYears,
    CurrentCompany = @CurrentCompany,
    Designation = @Designation,
    Bio = @Bio,
    LinkedInUrl = @LinkedInUrl
WHERE TrainerId = @TrainerId


END
GO
