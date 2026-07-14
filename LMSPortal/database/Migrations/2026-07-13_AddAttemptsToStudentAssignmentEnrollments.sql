-- Adds LMS.StudentAssignmentEnrollments.Attempts so assignment attempts can
-- be tracked (count + best score), letting certificate generation eventually
-- be gated by a verified pass rather than a client-supplied score.
-- Safe to run once per database (guarded with IF NOT EXISTS check).

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('LMS.StudentAssignmentEnrollments') AND name = 'Attempts'
)
BEGIN
    ALTER TABLE LMS.StudentAssignmentEnrollments
    ADD Attempts INT NOT NULL
        CONSTRAINT DF_StudentAssignmentEnrollments_Attempts DEFAULT 0;
END
GO
