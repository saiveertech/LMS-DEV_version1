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

            var (processedSlides, slidesError) = await ProcessSlidesAsync(request.Slides);

            if (slidesError != null)
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = slidesError
                };
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
                processedSlides!,
                createdById,
                createdByName,
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
        UpdateCourseRequest request,
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

            List<CourseSlideInput>? processedSlides = null;

            if (request.Slides != null)
            {
                var (slides, slidesError) = await ProcessSlidesAsync(request.Slides);

                if (slidesError != null)
                {
                    return new ServiceResponse
                    {
                        Success = false,
                        Message = slidesError
                    };
                }

                processedSlides = slides;
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
                courseIconUrl,
                processedSlides,
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

    //=========================================
    // Delete Course
    //=========================================

    public async Task<ServiceResponse> DeleteCourse(
        int courseId,
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

            var course = await _repo.GetCourseById(courseId);

            if (course == null)
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "Course Not Found."
                };
            }

            var deleted = await _repo.DeleteCourse(
                courseId,
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
                Message = "Course Deleted Successfully."
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

    private async Task<(List<CourseSlideInput>? Slides, string? Error)> ProcessSlidesAsync(
        List<CourseSlideRequest>? slides)
    {
        if (slides == null || slides.Count == 0)
        {
            return (new List<CourseSlideInput>(), null);
        }

        var processed = new List<CourseSlideInput>();

        foreach (var slide in slides)
        {
            if (string.IsNullOrWhiteSpace(slide.Title))
            {
                return (null, "Each slide requires a title.");
            }

            string mediaUrl;

            if (slide.MediaType == SlideMediaType.Video)
            {
                if (slide.VideoFile == null)
                {
                    return (null, $"Slide '{slide.Title}' requires a video file.");
                }

                var validationError = ValidateFile(
                    slide.VideoFile,
                    AllowedVideoExtensions,
                    MaxVideoSizeBytes,
                    $"Slide '{slide.Title}' video");

                if (validationError != null)
                {
                    return (null, validationError);
                }

                mediaUrl = await _blobStorageService.UploadFileAsync(
                    slide.VideoFile,
                    "course-slides");
            }
            else
            {
                if (string.IsNullOrWhiteSpace(slide.MediaUrl))
                {
                    return (null, $"Slide '{slide.Title}' requires a media URL.");
                }

                mediaUrl = slide.MediaUrl;
            }

            processed.Add(new CourseSlideInput
            {
                Title = slide.Title,
                Description = slide.Description,
                MediaType = slide.MediaType.ToString(),
                MediaUrl = mediaUrl,
                SortOrder = slide.SortOrder
            });
        }

        return (processed, null);
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
