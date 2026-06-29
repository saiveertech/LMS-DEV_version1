ALTER PROCEDURE LMS.SP_UpdateTrainer
(
    @TrainerId NVARCHAR(50),
    @FirstName NVARCHAR(100) = NULL,
    @LastName NVARCHAR(100) = NULL,
    @Email NVARCHAR(200) = NULL,
    @PhoneNumber NVARCHAR(50) = NULL,
    @ExperienceYears INT = NULL,
    @CurrentCompany NVARCHAR(200) = NULL,
    @Designation NVARCHAR(200) = NULL,
    @Bio NVARCHAR(MAX) = NULL,
    @LinkedInUrl NVARCHAR(500) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE LMS.Trainers
    SET
        FirstName = ISNULL(@FirstName, FirstName),
        LastName = ISNULL(@LastName, LastName),
        Email = ISNULL(@Email, Email),
        PhoneNumber = ISNULL(@PhoneNumber, PhoneNumber),
        ExperienceYears = ISNULL(@ExperienceYears, ExperienceYears),
        CurrentCompany = ISNULL(@CurrentCompany, CurrentCompany),
        Designation = ISNULL(@Designation, Designation),
        Bio = ISNULL(@Bio, Bio),
        LinkedInUrl = ISNULL(@LinkedInUrl, LinkedInUrl)
    WHERE TrainerId = @TrainerId;
END
GO