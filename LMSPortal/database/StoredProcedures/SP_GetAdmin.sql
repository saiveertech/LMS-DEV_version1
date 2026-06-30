CREATE PROCEDURE LMS.SP_GetAdminById
(
    @AdminId NVARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        AdminId,
        FirstName,
        LastName,
        Email,
        PhoneNumber,
        ExperienceYears,
        Skills,
        Bio,
        CreatedDate
    FROM LMS.Admin
    WHERE AdminId=@AdminId;
END
GO