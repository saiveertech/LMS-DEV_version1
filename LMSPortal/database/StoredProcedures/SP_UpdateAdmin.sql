ALTER PROCEDURE LMS.SP_UpdateAdmin
(
    @AdminId NVARCHAR(50),
    @FirstName NVARCHAR(100)=NULL,
    @LastName NVARCHAR(100)=NULL,
    @Email NVARCHAR(200)=NULL,
    @PhoneNumber NVARCHAR(50)=NULL,
    @ExperienceYears INT=NULL,
    @Skills NVARCHAR(MAX)=NULL,
    @Bio NVARCHAR(MAX)=NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE LMS.Admin
    SET
        FirstName = CASE WHEN @FirstName IS NULL THEN FirstName ELSE @FirstName END,
        LastName = CASE WHEN @LastName IS NULL THEN LastName ELSE @LastName END,
        Email = CASE WHEN @Email IS NULL THEN Email ELSE @Email END,
        PhoneNumber = CASE WHEN @PhoneNumber IS NULL THEN PhoneNumber ELSE @PhoneNumber END,
        ExperienceYears = CASE WHEN @ExperienceYears IS NULL THEN ExperienceYears ELSE @ExperienceYears END,
        Skills = CASE WHEN @Skills IS NULL THEN Skills ELSE @Skills END,
        Bio = CASE WHEN @Bio IS NULL THEN Bio ELSE @Bio END,
        UpdatedDate = GETDATE()
    WHERE AdminId = @AdminId;

    SELECT @@ROWCOUNT;
END
GO