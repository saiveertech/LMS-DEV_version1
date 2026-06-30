using System.ComponentModel.DataAnnotations;
using LMS.Application.Common;
using LMS.Application.Features.Auth.DTOs;

namespace LMS.Application.Features.Auth.Services.Student;

public class StudentService : IStudentService
{
    private readonly IStudentRepository _repo;

    public StudentService(IStudentRepository repo)
    {
        _repo = repo;
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
        string studentId)
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
}