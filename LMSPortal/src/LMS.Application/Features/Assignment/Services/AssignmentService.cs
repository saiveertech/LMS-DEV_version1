using LMS.Application.Common;
using LMS.Application.Features.Assignment.DTOs;
using Microsoft.AspNetCore.Http;

namespace LMS.Application.Features.Assignment.Services;

public class AssignmentService : IAssignmentService
{
    private static readonly string[] AllowedVideoExtensions = { ".mp4", ".mov", ".webm" };

    private static readonly string[] AllowedIconExtensions = { ".png", ".jpg", ".jpeg", ".webp" };

    private static readonly string[] AllowedCsvExtensions = { ".csv" };

    private const long MaxVideoSizeBytes = 500 * 1024 * 1024;

    private const long MaxIconSizeBytes = 5 * 1024 * 1024;

    private const long MaxCsvSizeBytes = 20 * 1024 * 1024;

    private readonly IAssignmentRepository _repo;
    private readonly IBlobStorageService _blobStorageService;

    public AssignmentService(
        IAssignmentRepository repo,
        IBlobStorageService blobStorageService)
    {
        _repo = repo;
        _blobStorageService = blobStorageService;
    }

    //=========================================
    // Create Assignment
    //=========================================

    public async Task<ServiceResponse> CreateAssignment(
        CreateAssignmentRequest request,
        string createdById,
        string createdByName,
        string createdByRole)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(createdById) ||
                string.IsNullOrWhiteSpace(createdByName) ||
                string.IsNullOrWhiteSpace(createdByRole))
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "Unable to determine the authenticated user."
                };
            }

            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "Title is required."
                };
            }

            if (request.Title.Length < 3)
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "Title should contain minimum 3 characters."
                };
            }

            if (request.CompletionTimeSeconds <= 0)
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "Completion Time Seconds must be greater than 0."
                };
            }

            if (request.PassPercentage < 0 || request.PassPercentage > 100)
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "Pass Percentage must be between 0 and 100."
                };
            }

            string introVideoUrl = string.Empty;
            string questionsCsvUrl = string.Empty;
            string assessmentIconUrl = string.Empty;

            if (request.IntroVideo != null)
            {
                var validationError = ValidateFile(
                    request.IntroVideo,
                    AllowedVideoExtensions,
                    MaxVideoSizeBytes,
                    "Intro Video");

                if (validationError != null)
                {
                    return new ServiceResponse
                    {
                        Success = false,
                        Message = validationError
                    };
                }

                introVideoUrl = await _blobStorageService.UploadFileAsync(
                    request.IntroVideo,
                    "assignment-intro-videos");
            }

            if (request.QuestionsCsv != null)
            {
                var validationError = ValidateFile(
                    request.QuestionsCsv,
                    AllowedCsvExtensions,
                    MaxCsvSizeBytes,
                    "Questions Csv");

                if (validationError != null)
                {
                    return new ServiceResponse
                    {
                        Success = false,
                        Message = validationError
                    };
                }

                questionsCsvUrl = await _blobStorageService.UploadFileAsync(
                    request.QuestionsCsv,
                    "assignment-questions-csv");
            }

            if (request.AssessmentIcon != null)
            {
                var validationError = ValidateFile(
                    request.AssessmentIcon,
                    AllowedIconExtensions,
                    MaxIconSizeBytes,
                    "Assessment Icon");

                if (validationError != null)
                {
                    return new ServiceResponse
                    {
                        Success = false,
                        Message = validationError
                    };
                }

                assessmentIconUrl = await _blobStorageService.UploadFileAsync(
                    request.AssessmentIcon,
                    "assignment-icons");
            }

            // Parse the answer key up front so a malformed CSV fails the
            // whole request before the assignment is even created.
            List<QuestionImportRow>? parsedQuestions = null;

            if (request.QuestionsCsv != null)
            {
                var (rows, parseError) = await ParseQuestionsCsv(request.QuestionsCsv);

                if (parseError != null)
                {
                    return new ServiceResponse { Success = false, Message = parseError };
                }

                parsedQuestions = rows;
            }

            var result = await _repo.CreateAssignment(
                request,
                introVideoUrl,
                questionsCsvUrl,
                assessmentIconUrl,
                createdById,
                createdByName,
                createdByRole);

            // Auto-import the answer key — the trainer only ever uploads the
            // CSV once, at create time (or re-uploads it on update below).
            if (parsedQuestions != null && result is AssignmentResponse created)
            {
                await _repo.ReplaceQuestions(created.AssignmentId, parsedQuestions);
            }

            return new ServiceResponse
            {
                Success = true,
                Message = "Assignment Created Successfully.",
                Data = result
            };
        }
        catch (Exception ex)
        {
            return new ServiceResponse
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    //=========================================
    // Get Assignments
    //=========================================

    public async Task<object?> GetAssignments(int? assignmentId = null)
    {
        return await _repo.GetAssignments(assignmentId);
    }

    //=========================================
    // Update Assignment
    //=========================================

    public async Task<ServiceResponse> UpdateAssignment(
        int assignmentId,
        UpdateAssignmentRequest request,
        string editedById,
        string editedByName,
        string editedByRole)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(editedById) ||
                string.IsNullOrWhiteSpace(editedByName) ||
                string.IsNullOrWhiteSpace(editedByRole))
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "Unable to determine the authenticated user."
                };
            }

            var assignment = await _repo.GetAssignments(assignmentId);

            if (assignment == null)
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "Assignment Not Found."
                };
            }

            if (request.Title != null && request.Title.Trim().Length < 3)
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "Title should contain minimum 3 characters."
                };
            }

            if (request.CompletionTimeSeconds.HasValue &&
                request.CompletionTimeSeconds.Value <= 0)
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "Completion Time Seconds must be greater than 0."
                };
            }

            if (request.PassPercentage.HasValue &&
                (request.PassPercentage.Value < 0 || request.PassPercentage.Value > 100))
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "Pass Percentage must be between 0 and 100."
                };
            }

            string? introVideoUrl = null;
            string? questionsCsvUrl = null;
            string? assessmentIconUrl = null;

            if (request.IntroVideo != null)
            {
                var validationError = ValidateFile(
                    request.IntroVideo,
                    AllowedVideoExtensions,
                    MaxVideoSizeBytes,
                    "Intro Video");

                if (validationError != null)
                {
                    return new ServiceResponse
                    {
                        Success = false,
                        Message = validationError
                    };
                }

                introVideoUrl = await _blobStorageService.UploadFileAsync(
                    request.IntroVideo,
                    "assignment-intro-videos");
            }

            if (request.QuestionsCsv != null)
            {
                var validationError = ValidateFile(
                    request.QuestionsCsv,
                    AllowedCsvExtensions,
                    MaxCsvSizeBytes,
                    "Questions Csv");

                if (validationError != null)
                {
                    return new ServiceResponse
                    {
                        Success = false,
                        Message = validationError
                    };
                }

                questionsCsvUrl = await _blobStorageService.UploadFileAsync(
                    request.QuestionsCsv,
                    "assignment-questions-csv");
            }

            if (request.AssessmentIcon != null)
            {
                var validationError = ValidateFile(
                    request.AssessmentIcon,
                    AllowedIconExtensions,
                    MaxIconSizeBytes,
                    "Assessment Icon");

                if (validationError != null)
                {
                    return new ServiceResponse
                    {
                        Success = false,
                        Message = validationError
                    };
                }

                assessmentIconUrl = await _blobStorageService.UploadFileAsync(
                    request.AssessmentIcon,
                    "assignment-icons");
            }

            // A re-uploaded CSV means the trainer wants to replace the
            // answer key entirely.
            List<QuestionImportRow>? parsedQuestions = null;

            if (request.QuestionsCsv != null)
            {
                var (rows, parseError) = await ParseQuestionsCsv(request.QuestionsCsv);

                if (parseError != null)
                {
                    return new ServiceResponse { Success = false, Message = parseError };
                }

                parsedQuestions = rows;
            }

            var updated = await _repo.UpdateAssignment(
                assignmentId,
                request,
                introVideoUrl,
                questionsCsvUrl,
                assessmentIconUrl,
                editedById,
                editedByName,
                editedByRole);

            if (!updated)
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "Update Failed."
                };
            }

            if (parsedQuestions != null)
            {
                await _repo.ReplaceQuestions(assignmentId, parsedQuestions);
            }

            return new ServiceResponse
            {
                Success = true,
                Message = "Assignment Updated Successfully.",
                Data = await _repo.GetAssignments(assignmentId)
            };
        }
        catch (Exception ex)
        {
            return new ServiceResponse
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    //=========================================
    // Delete Assignment
    //=========================================

    public async Task<ServiceResponse> DeleteAssignment(
        int assignmentId,
        string deletedById,
        string deletedByName,
        string deletedByRole)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(deletedById) ||
                string.IsNullOrWhiteSpace(deletedByName) ||
                string.IsNullOrWhiteSpace(deletedByRole))
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "Unable to determine the authenticated user."
                };
            }

            var assignment = await _repo.GetAssignments(assignmentId);

            if (assignment == null)
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "Assignment Not Found."
                };
            }

            var deleted = await _repo.DeleteAssignment(
                assignmentId,
                deletedById,
                deletedByName,
                deletedByRole);

            if (!deleted)
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "Delete Failed."
                };
            }

            return new ServiceResponse
            {
                Success = true,
                Message = "Assignment Deleted Successfully."
            };
        }
        catch (Exception ex)
        {
            return new ServiceResponse
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    private static string? ValidateFile(
        IFormFile file,
        string[] allowedExtensions,
        long maxSizeBytes,
        string fieldName)
    {
        if (file.Length <= 0)
        {
            return $"{fieldName} file is empty.";
        }

        if (file.Length > maxSizeBytes)
        {
            return $"{fieldName} exceeds the maximum allowed size of {maxSizeBytes / (1024 * 1024)} MB.";
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
        {
            return $"{fieldName} must be one of the following types: {string.Join(", ", allowedExtensions)}.";
        }

        return null;
    }

    //=========================================
    // Get Questions
    //=========================================

    public Task<List<AssignmentQuestionResponse>> GetQuestions(int assignmentId)
    {
        return _repo.GetQuestions(assignmentId);
    }

    public async Task<List<AssignmentQuestionForStudentResponse>> GetQuestionsForStudent(int assignmentId)
    {
        var questions = await _repo.GetQuestions(assignmentId);

        return questions.Select(q => new AssignmentQuestionForStudentResponse
        {
            QuestionId = q.QuestionId,
            QuestionText = q.QuestionText,
            OptionA = q.OptionA,
            OptionB = q.OptionB,
            OptionC = q.OptionC,
            OptionD = q.OptionD,
            Marks = q.Marks,
            SortOrder = q.SortOrder
        }).ToList();
    }

    //=========================================
    // CSV Answer-Key Parsing
    //=========================================
    // Expected header: QuestionText,OptionA,OptionB,OptionC,OptionD,CorrectOption,Marks
    // Marks column is optional per row — defaults to 1 if omitted/blank.

    private static async Task<(List<QuestionImportRow> Rows, string? Error)> ParseQuestionsCsv(IFormFile file)
    {
        using var reader = new StreamReader(file.OpenReadStream());

        var text = await reader.ReadToEndAsync();

        var lines = text.Split('\n')
            .Select(l => l.TrimEnd('\r'))
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToList();

        if (lines.Count <= 1)
        {
            return (new List<QuestionImportRow>(), "Questions CSV must contain a header row plus at least one question.");
        }

        var rows = new List<QuestionImportRow>();

        for (int i = 1; i < lines.Count; i++)
        {
            var fields = SplitCsvLine(lines[i]);
            var lineNumber = i + 1;

            if (fields.Count < 6)
            {
                return (rows, $"CSV line {lineNumber}: expected at least 6 columns (QuestionText, OptionA-D, CorrectOption), got {fields.Count}.");
            }

            var correctOption = fields[5].Trim().ToUpperInvariant();

            if (correctOption != "A" && correctOption != "B" && correctOption != "C" && correctOption != "D")
            {
                return (rows, $"CSV line {lineNumber}: CorrectOption must be A, B, C, or D — got '{fields[5]}'.");
            }

            decimal marks = 1;

            if (fields.Count >= 7 && !string.IsNullOrWhiteSpace(fields[6]))
            {
                if (!decimal.TryParse(fields[6], out marks) || marks <= 0)
                {
                    return (rows, $"CSV line {lineNumber}: Marks must be a positive number — got '{fields[6]}'.");
                }
            }

            rows.Add(new QuestionImportRow
            {
                QuestionText = fields[0].Trim(),
                OptionA = fields[1].Trim(),
                OptionB = fields[2].Trim(),
                OptionC = fields[3].Trim(),
                OptionD = fields[4].Trim(),
                CorrectOption = correctOption,
                Marks = marks
            });
        }

        return (rows, null);
    }

    // Minimal quote-aware CSV splitter — handles quoted fields containing commas.
    private static List<string> SplitCsvLine(string line)
    {
        var fields = new List<string>();
        var current = new System.Text.StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    current.Append(c);
                }
            }
            else if (c == '"')
            {
                inQuotes = true;
            }
            else if (c == ',')
            {
                fields.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        fields.Add(current.ToString());

        return fields;
    }
}
