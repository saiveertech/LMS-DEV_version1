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

            var result = await _repo.CreateAssignment(
                request,
                introVideoUrl,
                questionsCsvUrl,
                assessmentIconUrl,
                createdById,
                createdByName,
                createdByRole);

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
        UpdateAssignmentRequest request)
    {
        try
        {
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

            var updated = await _repo.UpdateAssignment(
                assignmentId,
                request,
                introVideoUrl,
                questionsCsvUrl,
                assessmentIconUrl);

            if (!updated)
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "Update Failed."
                };
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

    public async Task<ServiceResponse> DeleteAssignment(int assignmentId)
    {
        try
        {
            var assignment = await _repo.GetAssignments(assignmentId);

            if (assignment == null)
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "Assignment Not Found."
                };
            }

            var deleted = await _repo.DeleteAssignment(assignmentId);

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
}
