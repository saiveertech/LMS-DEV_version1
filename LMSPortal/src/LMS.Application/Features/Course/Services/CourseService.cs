using System.Text.Json;
using LMS.Application.Common;
using LMS.Application.Features.Course.DTOs;
using Microsoft.AspNetCore.Http;

namespace LMS.Application.Features.Course.Services;

public class CourseService : ICourseService
{
    private static readonly string[] AllowedVideoExtensions = { ".mp4", ".mov", ".webm" };

    private static readonly string[] AllowedIconExtensions = { ".png", ".jpg", ".jpeg", ".webp" };

    private const long MaxVideoSizeBytes = 500 * 1024 * 1024;

    private const long MaxIconSizeBytes = 5 * 1024 * 1024;

    private readonly ICourseRepository _repo;
    private readonly IBlobStorageService _blobStorageService;

    public CourseService(
        ICourseRepository repo,
        IBlobStorageService blobStorageService)
    {
        _repo = repo;
        _blobStorageService = blobStorageService;
    }

    //=========================================
    // Register Course
    //=========================================

    public async Task<ServiceResponse> RegisterCourse(
        CreateCourseRequest request,
        string createdBy,
        string createdByRole)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(createdBy) ||
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

            if (!string.IsNullOrWhiteSpace(request.SlidesJson))
            {
                try
                {
                    using var _ = JsonDocument.Parse(request.SlidesJson);
                }
                catch (JsonException)
                {
                    return new ServiceResponse
                    {
                        Success = false,
                        Message = "Slides Json is not valid JSON."
                    };
                }
            }

            string introVideoUrl = string.Empty;
            string courseIconUrl = string.Empty;

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
                    "intro-videos");
            }

            if (request.CourseIcon != null)
            {
                var validationError = ValidateFile(
                    request.CourseIcon,
                    AllowedIconExtensions,
                    MaxIconSizeBytes,
                    "Course Icon");

                if (validationError != null)
                {
                    return new ServiceResponse
                    {
                        Success = false,
                        Message = validationError
                    };
                }

                courseIconUrl = await _blobStorageService.UploadFileAsync(
                    request.CourseIcon,
                    "course-icons");
            }

            var result = await _repo.RegisterCourse(
                request,
                introVideoUrl,
                courseIconUrl,
                createdBy,
                createdByRole);

            return new ServiceResponse
            {
                Success = true,
                Message = "Course Created Successfully.",
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
    // Get Course
    //=========================================

    public async Task<object?> GetCourseById(int? courseId = null)
    {
        return await _repo.GetCourseById(courseId);
    }

    //=========================================
    // Update Course
    //=========================================

    public async Task<ServiceResponse> UpdateCourse(
        int courseId,
        UpdateCourseRequest request)
    {
        try
        {
            var course = await _repo.GetCourseById(courseId);

            if (course == null)
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "Course Not Found."
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

            if (!string.IsNullOrWhiteSpace(request.SlidesJson))
            {
                try
                {
                    using var _ = JsonDocument.Parse(request.SlidesJson);
                }
                catch (JsonException)
                {
                    return new ServiceResponse
                    {
                        Success = false,
                        Message = "Slides Json is not valid JSON."
                    };
                }
            }

            string? introVideoUrl = null;
            string? courseIconUrl = null;

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
                    "intro-videos");
            }

            if (request.CourseIcon != null)
            {
                var validationError = ValidateFile(
                    request.CourseIcon,
                    AllowedIconExtensions,
                    MaxIconSizeBytes,
                    "Course Icon");

                if (validationError != null)
                {
                    return new ServiceResponse
                    {
                        Success = false,
                        Message = validationError
                    };
                }

                courseIconUrl = await _blobStorageService.UploadFileAsync(
                    request.CourseIcon,
                    "course-icons");
            }

            var updated = await _repo.UpdateCourse(
                courseId,
                request,
                introVideoUrl,
                courseIconUrl);

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
                Message = "Course Updated Successfully.",
                Data = await _repo.GetCourseById(courseId)
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
