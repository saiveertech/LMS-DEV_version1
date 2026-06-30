ALTER PROCEDURE LMS.SP_UpdateStudent
(
    @StudentId NVARCHAR(50),
    @FirstName NVARCHAR(100)=NULL,
    @LastName NVARCHAR(100)=NULL,
    @Email NVARCHAR(200)=NULL,
    @PhoneNumber NVARCHAR(50)=NULL,
    @EducationDetails NVARCHAR(MAX)=NULL,
    @AreaOfInterest NVARCHAR(MAX)=NULL
)
AS
BEGIN

SET NOCOUNT ON;

UPDATE LMS.Students
SET
    FirstName = ISNULL(@FirstName, FirstName),
    LastName = ISNULL(@LastName, LastName),
    Email = ISNULL(@Email, Email),
    PhoneNumber = ISNULL(@PhoneNumber, PhoneNumber),
    EducationDetails = ISNULL(@EducationDetails, EducationDetails),
    AreaOfInterest = ISNULL(@AreaOfInterest, AreaOfInterest)

WHERE StudentId=@StudentId;

SELECT @@ROWCOUNT;

END
GO