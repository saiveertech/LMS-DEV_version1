ALTER PROCEDURE LMS.SP_GetAdminById
(
    @AdminId NVARCHAR(50) = NULL
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
    WHERE (@AdminId IS NULL OR AdminId = @AdminId)
    ORDER BY CreatedDate DESC;
END
GO
