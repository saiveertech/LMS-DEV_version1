CREATE PROCEDURE LMS.SP_GetTrainerById
(
@TrainerId NVARCHAR(50)
)
AS
BEGIN


SELECT *
FROM LMS.Trainers
WHERE TrainerId = @TrainerId


END
GO
