CREATE PROCEDURE LMS.SP_GetAssignmentStudentTracking
(
    @AssignmentId         INT,
    @Status               NVARCHAR(50) = NULL,   -- Enrolled | In Progress | Completed
    @StudentId            NVARCHAR(50) = NULL,
    @CertificateGenerated BIT          = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        e.StudentId,
        s.FirstName + ' ' + s.LastName                             AS StudentName,
        s.Email,
        e.AssignmentId,
        a.Title                                                    AS AssignmentTitle,
        e.EnrollmentDate,
        CASE WHEN aa.Id IS NOT NULL THEN 'TrainerAssigned' ELSE 'Self' END AS EnrollmentSource,
        aa.AssignedByName,
        e.AssignmentStatus,
        e.AssessmentScore,
        a.PassPercentage,
        e.Attempts,
        CAST(
            CASE WHEN cert.CertificateId IS NOT NULL THEN 1 ELSE 0 END
        AS BIT)                                                     AS CertificateGenerated,
        cert.CertificateId,
        cert.IssuedDate                                             AS CertificateIssueDate,
        cert.CertificateUrl,
        e.CreatedAt,
        e.UpdatedDate
    FROM LMS.StudentAssignmentEnrollments e
    INNER JOIN LMS.Students s
        ON e.StudentId = s.StudentId
    INNER JOIN LMS.Assignments a
        ON e.AssignmentId = a.AssignmentId
    LEFT JOIN LMS.Certificates cert
        ON cert.StudentId = e.StudentId
       AND cert.AssignmentId = e.AssignmentId
       AND cert.CertificateType = 'Assignment'
    LEFT JOIN LMS.AssignmentAllocations aa
        ON aa.StudentId = e.StudentId
       AND aa.AssignmentId = e.AssignmentId
    WHERE e.AssignmentId = @AssignmentId
      AND (@Status IS NULL OR e.AssignmentStatus = @Status)
      AND (@StudentId IS NULL OR e.StudentId = @StudentId)
      AND (
            @CertificateGenerated IS NULL
            OR (@CertificateGenerated = 1 AND cert.CertificateId IS NOT NULL)
            OR (@CertificateGenerated = 0 AND cert.CertificateId IS NULL)
          )
    ORDER BY e.EnrollmentDate DESC;

END
GO
