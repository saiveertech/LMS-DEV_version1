using System.ComponentModel.DataAnnotations;
using LMS.Application.Common;
using LMS.Application.Features.Assignment.DTOs;
using LMS.Application.Features.Assignment.Services;
using LMS.Application.Features.Auth.DTOs;
using LMS.Application.Features.Certificate.Services;
using LMS.Application.Features.CourseStudentTracking.DTOs;

namespace LMS.Application.Features.Auth.Services.Student;

public class StudentService : IStudentService
{
    private readonly IStudentRepository _repo;
    private readonly ICertificateService _certificateService;
    private readonly IAssignmentRepository _assignmentRepo;

    public StudentService(
        IStudentRepository repo,
        ICertificateService certificateService,
        IAssignmentRepository assignmentRepo)
    {
        _repo = repo;
        _certificateService = certificateService;
        _assignmentRepo = assignmentRepo;
    }

    //=========================================
    // Register Student
    //=========================================

    public async Task<ServiceResponse> RegisterStudent(
        RegisterStudentRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.FirstName))
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "First Name is required."
                };
            }

            if (string.IsNullOrWhiteSpace(request.LastName))
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "Last Name is required."
                };
            }

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "Email is required."
                };
            }

            if (string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "Phone Number is required."
                };
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "Password is required."
                };
            }

            if (!new EmailAddressAttribute().IsValid(request.Email))
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "Invalid Email Address."
                };
            }

            if (request.PhoneNumber.Length != 10)
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "Phone Number should contain 10 digits."
                };
            }

            if (!request.PhoneNumber.All(char.IsDigit))
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "Phone Number should contain only digits."
                };
            }

            if (request.Password.Length < 8)
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "Password should be minimum 8 characters."
                };
            }

            if (request.FirstName.Length < 2)
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "First Name should contain minimum 2 characters."
                };
            }

            if (request.LastName.Length < 2)
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "Last Name should contain minimum 2 characters."
                };
            }

            var result =
                await _repo.RegisterStudent(request);

            return new ServiceResponse
            {
                Success = true,
                Message = "Student Registered Successfully.",
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
    // Get Student
    //=========================================

    public async Task<object?> GetStudentById(
        string? studentId = null)
    {
        return await _repo.GetStudentById(studentId);
    }

    //=========================================
    // Update Student
    //=========================================

    public async Task<ServiceResponse> UpdateStudent(
        string studentId,
        UpdateStudentRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(studentId))
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "Student Id is required."
                };
            }

            var student =
                await _repo.GetStudentById(studentId);

            if (student == null)
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "Student Not Found."
                };
            }

            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                if (!new EmailAddressAttribute().IsValid(request.Email))
                {
                    return new ServiceResponse
                    {
                        Success = false,
                        Message = "Invalid Email Address."
                    };
                }
            }

            if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                if (request.PhoneNumber.Length != 10)
                {
                    return new ServiceResponse
                    {
                        Success = false,
                        Message = "Phone Number should contain 10 digits."
                    };
                }

                if (!request.PhoneNumber.All(char.IsDigit))
                {
                    return new ServiceResponse
                    {
                        Success = false,
                        Message = "Phone Number should contain only digits."
                    };
                }
            }

            bool result =
                await _repo.UpdateStudent(
                    studentId,
                    request);

            if (!result)
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
                Message = "Student Updated Successfully."
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
    // Enroll Student in a Course
    //=========================================

    public async Task<ServiceResponse> EnrollCourse(EnrollCourseRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.StudentId))
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "Student ID is required."
                };
            }

            if (request.CourseId <= 0)
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "A valid Course ID is required."
                };
            }

            var result = await _repo.EnrollCourse(request);

            return new ServiceResponse
            {
                Success = true,
                Message = "Student enrolled in course successfully.",
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
    // Get Enrolled Courses for a Student
    //=========================================

    public async Task<object?> GetEnrolledCourses(string studentId)
    {
        return await _repo.GetEnrolledCourses(studentId);
    }

    //=========================================
    // Enroll Student in an Assignment
    //=========================================

    public async Task<ServiceResponse> EnrollAssignment(EnrollAssignmentRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.StudentId))
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "Student ID is required."
                };
            }

            if (request.AssignmentId <= 0)
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "A valid Assignment ID is required."
                };
            }

            var result = await _repo.EnrollAssignment(request);

            return new ServiceResponse
            {
                Success = true,
                Message = "Student enrolled in assignment successfully.",
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
    // Get Enrolled Assignments for a Student
    //=========================================

    public async Task<object?> GetEnrolledAssignments(string studentId)
    {
        return await _repo.GetEnrolledAssignments(studentId);
    }

    //=========================================
    // Submit Assignment Attempt
    //=========================================

    public async Task<ServiceResponse> SubmitAssignmentAttempt(SubmitAssignmentAttemptRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.StudentId))
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "Student ID is required."
                };
            }

            if (request.AssignmentId <= 0)
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "A valid Assignment ID is required."
                };
            }

            if (request.Score < 0 || request.Score > 100)
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "Score must be between 0 and 100."
                };
            }

            var result = await _repo.SubmitAssignmentAttempt(request);

            var message = "Assignment attempt recorded successfully.";

            var resultType = result.GetType();
            var assignmentStatus = resultType.GetProperty("AssignmentStatus")?.GetValue(result) as string;

            // Auto-issue the assignment certificate the instant a passing
            // score is reached — no manual trainer/admin trigger. Idempotent:
            // a later attempt after already passing just fails the
            // "already exists" check inside GenerateAssignmentCertificateAsync
            // and is ignored here.
            if (assignmentStatus == "Completed")
            {
                var assessmentScore = Convert.ToDecimal(
                    resultType.GetProperty("AssessmentScore")?.GetValue(result) ?? 0m);

                var certResult = await _certificateService.GenerateAssignmentCertificateAsync(
                    request.StudentId,
                    request.AssignmentId,
                    assessmentScore,
                    DateTime.UtcNow,
                    "system",
                    "System",
                    "System");

                message = certResult.Success
                    ? "Assignment attempt recorded — passed and certificate issued."
                    : "Assignment attempt recorded — passed.";
            }

            return new ServiceResponse
            {
                Success = true,
                Message = message,
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
    // Assign Course To Student (trainer/admin-initiated)
    //=========================================

    public async Task<ServiceResponse> AssignCourseToStudent(
        string studentId, int courseId,
        string assignedById, string assignedByName, string assignedByRole)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(studentId))
            {
                return new ServiceResponse { Success = false, Message = "Student ID is required." };
            }

            if (courseId <= 0)
            {
                return new ServiceResponse { Success = false, Message = "A valid Course ID is required." };
            }

            var enrollment = await _repo.EnrollCourse(
                new EnrollCourseRequest { StudentId = studentId, CourseId = courseId });

            await _repo.RecordCourseAssignment(
                studentId, courseId, assignedById, assignedByName, assignedByRole);

            return new ServiceResponse
            {
                Success = true,
                Message = "Course assigned to student successfully.",
                Data = enrollment
            };
        }
        catch (Exception ex)
        {
            return new ServiceResponse { Success = false, Message = ex.Message };
        }
    }

    //=========================================
    // Assign Assignment To Student (trainer/admin-initiated)
    //=========================================

    public async Task<ServiceResponse> AssignAssignmentToStudent(
        string studentId, int assignmentId,
        string assignedById, string assignedByName, string assignedByRole)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(studentId))
            {
                return new ServiceResponse { Success = false, Message = "Student ID is required." };
            }

            if (assignmentId <= 0)
            {
                return new ServiceResponse { Success = false, Message = "A valid Assignment ID is required." };
            }

            var enrollment = await _repo.EnrollAssignment(
                new EnrollAssignmentRequest { StudentId = studentId, AssignmentId = assignmentId });

            await _repo.RecordAssignmentAllocation(
                studentId, assignmentId, assignedById, assignedByName, assignedByRole);

            return new ServiceResponse
            {
                Success = true,
                Message = "Assignment assigned to student successfully.",
                Data = enrollment
            };
        }
        catch (Exception ex)
        {
            return new ServiceResponse { Success = false, Message = ex.Message };
        }
    }

    //=========================================
    // Get Assigned Courses / Assignments for a Student (trainer/admin audit trail)
    //=========================================

    public async Task<object?> GetAssignedCourses(string studentId)
    {
        return await _repo.GetAssignedCourses(studentId);
    }

    public async Task<object?> GetAssignedAssignments(string studentId)
    {
        return await _repo.GetAssignedAssignments(studentId);
    }

    //=========================================
    // Get Recommended Courses for a Student
    //=========================================

    public async Task<object?> GetRecommendedCourses(string studentId, int topN)
    {
        return await _repo.GetRecommendedCourses(studentId, topN);
    }

    //=========================================
    // Get Recommended Assignments for a Student
    //=========================================

    public async Task<object?> GetRecommendedAssignments(string studentId, int topN)
    {
        return await _repo.GetRecommendedAssignments(studentId, topN);
    }

    //=========================================
    // Complete Course Slide
    //=========================================

    public async Task<ServiceResponse> CompleteCourseSlide(string studentId, int slideId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(studentId))
            {
                return new ServiceResponse { Success = false, Message = "Student ID is required." };
            }

            if (slideId <= 0)
            {
                return new ServiceResponse { Success = false, Message = "A valid Slide ID is required." };
            }

            var result = await _repo.CompleteCourseSlide(studentId, slideId);

            if (result == null)
            {
                return new ServiceResponse { Success = false, Message = "Slide or enrollment not found." };
            }

            var message = "Slide marked complete.";

            // Auto-issue the course certificate the instant completion hits
            // 100% — same idempotent pattern as the manual progress endpoint.
            if (result.CourseStatus == "Completed")
            {
                var certResult = await _certificateService.GenerateCourseCertificateAsync(
                    studentId,
                    result.CourseId,
                    result.CompletionPercentage,
                    DateTime.UtcNow,
                    "system",
                    "System",
                    "System");

                message = certResult.Success
                    ? "Slide marked complete — course finished and certificate issued."
                    : "Slide marked complete — course finished.";
            }

            return new ServiceResponse
            {
                Success = true,
                Message = message,
                Data = result
            };
        }
        catch (Exception ex)
        {
            return new ServiceResponse { Success = false, Message = ex.Message };
        }
    }

    //=========================================
    // Get Course Resume Point
    //=========================================

    public Task<List<CourseSlideProgressResponse>> GetCourseResume(string studentId, int courseId)
    {
        return _repo.GetCourseResume(studentId, courseId);
    }

    //=========================================
    // Submit Assignment Answers (real evaluation against the answer key)
    //=========================================

    public async Task<ServiceResponse> SubmitAssignmentAnswers(SubmitAssignmentAnswersRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.StudentId))
            {
                return new ServiceResponse { Success = false, Message = "Student ID is required." };
            }

            if (request.AssignmentId <= 0)
            {
                return new ServiceResponse { Success = false, Message = "A valid Assignment ID is required." };
            }

            if (request.Answers.Count == 0)
            {
                return new ServiceResponse { Success = false, Message = "At least one answer is required." };
            }

            var questions = await _assignmentRepo.GetQuestions(request.AssignmentId);

            if (questions.Count == 0)
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "No answer key configured for this assignment."
                };
            }

            var submittedByQuestion = request.Answers.ToDictionary(a => a.QuestionId, a => a.SelectedOption);

            var results = new List<AssignmentAnswerResultItem>();
            var answersToSave = new List<(int QuestionId, string SelectedOption, bool IsCorrect)>();

            decimal correctMarks = 0;
            decimal totalMarks = 0;
            int correctCount = 0;

            foreach (var question in questions)
            {
                totalMarks += question.Marks;

                submittedByQuestion.TryGetValue(question.QuestionId, out var selectedOption);

                var isCorrect = !string.IsNullOrWhiteSpace(selectedOption) &&
                    string.Equals(selectedOption.Trim(), question.CorrectOption, StringComparison.OrdinalIgnoreCase);

                if (isCorrect)
                {
                    correctMarks += question.Marks;
                    correctCount++;
                }

                results.Add(new AssignmentAnswerResultItem
                {
                    QuestionId = question.QuestionId,
                    SelectedOption = selectedOption,
                    CorrectOption = question.CorrectOption,
                    IsCorrect = isCorrect
                });

                // Unanswered questions aren't recorded as a submitted answer row.
                if (!string.IsNullOrWhiteSpace(selectedOption))
                {
                    answersToSave.Add((question.QuestionId, selectedOption.Trim().ToUpperInvariant(), isCorrect));
                }
            }

            var scorePercentage = totalMarks > 0
                ? Math.Round(correctMarks / totalMarks * 100, 2)
                : 0;

            var attemptNumber = await _repo.SaveStudentAnswers(
                request.StudentId, request.AssignmentId, answersToSave);

            // Reuse the existing attempt flow entirely — best-score tracking,
            // pass-status derivation, and certificate auto-trigger all happen
            // exactly as they would for a directly-submitted score.
            var attemptResult = await SubmitAssignmentAttempt(new SubmitAssignmentAttemptRequest
            {
                StudentId = request.StudentId,
                AssignmentId = request.AssignmentId,
                Score = scorePercentage
            });

            var response = new SubmitAssignmentAnswersResponse
            {
                AssignmentId = request.AssignmentId,
                AttemptNumber = attemptNumber,
                CorrectCount = correctCount,
                TotalQuestions = questions.Count,
                CorrectMarks = correctMarks,
                TotalMarks = totalMarks,
                ScorePercentage = scorePercentage,
                Passed = attemptResult.Message.Contains("passed", StringComparison.OrdinalIgnoreCase),
                Results = results
            };

            return new ServiceResponse
            {
                Success = true,
                Message = attemptResult.Message,
                Data = response
            };
        }
        catch (Exception ex)
        {
            return new ServiceResponse { Success = false, Message = ex.Message };
        }
    }
}