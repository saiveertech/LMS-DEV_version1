using ClosedXML.Excel;
using LMS.Application.Common;
using LMS.Application.Features.Assignment.DTOs;
using Microsoft.AspNetCore.Http;

namespace LMS.Application.Features.Assignment.Services;

public class AssignmentService : IAssignmentService
{
    private static readonly string[] AllowedVideoExtensions = { ".mp4", ".mov", ".webm" };

    private static readonly string[] AllowedIconExtensions = { ".png", ".jpg", ".jpeg", ".webp" };

    private static readonly string[] AllowedQuestionsFileExtensions = { ".xlsx" };

    private const long MaxVideoSizeBytes = 500 * 1024 * 1024;

    private const long MaxIconSizeBytes = 5 * 1024 * 1024;

    private const long MaxQuestionsFileSizeBytes = 20 * 1024 * 1024;

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
                    AllowedQuestionsFileExtensions,
                    MaxQuestionsFileSizeBytes,
                    "Questions Excel");

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

            // Parse the answer key up front so a malformed Excel file fails
            // the whole request before the assignment is even created.
            List<QuestionImportRow>? parsedQuestions = null;

            if (request.QuestionsCsv != null)
            {
                var (rows, parseError) = await ParseQuestionsExcel(request.QuestionsCsv);

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
            // Excel file once, at create time (or re-uploads it on update below).
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
                    AllowedQuestionsFileExtensions,
                    MaxQuestionsFileSizeBytes,
                    "Questions Excel");

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

            // A re-uploaded Excel file means the trainer wants to replace
            // the answer key entirely.
            List<QuestionImportRow>? parsedQuestions = null;

            if (request.QuestionsCsv != null)
            {
                var (rows, parseError) = await ParseQuestionsExcel(request.QuestionsCsv);

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
    // Excel Answer-Key Parsing
    //=========================================
    // Expected columns (row 1 = header, read by position not by header text):
    // A=id (ignored), B=prompt, C=optionA, D=optionB, E=optionC, F=optionD, G=correctKey.
    // Marks isn't part of this layout — every imported question defaults to 1 mark.

    private static Task<(List<QuestionImportRow> Rows, string? Error)> ParseQuestionsExcel(IFormFile file)
    {
        using var stream = file.OpenReadStream();
        using var workbook = new XLWorkbook(stream);

        var worksheet = workbook.Worksheets.First();
        var lastRowUsed = worksheet.LastRowUsed();
        var rows = new List<QuestionImportRow>();

        if (lastRowUsed == null || lastRowUsed.RowNumber() <= 1)
        {
            return Task.FromResult<(List<QuestionImportRow>, string?)>(
                (rows, "Questions Excel file must contain a header row plus at least one question."));
        }

        for (int rowNumber = 2; rowNumber <= lastRowUsed.RowNumber(); rowNumber++)
        {
            var row = worksheet.Row(rowNumber);

            var prompt = row.Cell(2).GetString().Trim();
            var optionA = row.Cell(3).GetString().Trim();
            var optionB = row.Cell(4).GetString().Trim();
            var optionC = row.Cell(5).GetString().Trim();
            var optionD = row.Cell(6).GetString().Trim();
            var correctKeyRaw = row.Cell(7).GetString().Trim();
            var correctKey = correctKeyRaw.ToUpperInvariant();

            // Skip fully blank trailing rows rather than treating them as errors.
            if (string.IsNullOrWhiteSpace(prompt) && string.IsNullOrWhiteSpace(correctKeyRaw))
            {
                continue;
            }

            if (correctKey != "A" && correctKey != "B" && correctKey != "C" && correctKey != "D")
            {
                return Task.FromResult<(List<QuestionImportRow>, string?)>(
                    (rows, $"Excel row {rowNumber}: correctKey must be A, B, C, or D — got '{correctKeyRaw}'."));
            }

            rows.Add(new QuestionImportRow
            {
                QuestionText = prompt,
                OptionA = optionA,
                OptionB = optionB,
                OptionC = optionC,
                OptionD = optionD,
                CorrectOption = correctKey,
                Marks = 1
            });
        }

        return Task.FromResult<(List<QuestionImportRow>, string?)>((rows, null));
    }
}
