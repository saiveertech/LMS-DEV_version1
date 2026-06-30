using System.ComponentModel.DataAnnotations;
using LMS.Application.Common;
using LMS.Application.Features.Auth.DTOs;

namespace LMS.Application.Features.Auth.Services.Trainer;

public class TrainerService : ITrainerService
{
    private readonly ITrainerRepository _repo;

    public TrainerService(ITrainerRepository repo)
    {
        _repo = repo;
    }

    public async Task<ServiceResponse> RegisterTrainer(
        RegisterTrainerRequest request)
    {
        try
        {
            //=========================================
            // Required Field Validation
            //=========================================

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

            //=========================================
            // Email Validation
            //=========================================

            if (!new EmailAddressAttribute().IsValid(request.Email))
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "Invalid Email Address."
                };
            }

            //=========================================
            // Phone Validation
            //=========================================

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

            //=========================================
            // Password Validation
            //=========================================

            if (request.Password.Length < 8)
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "Password should contain minimum 8 characters."
                };
            }

            //=========================================
            // First Name Validation
            //=========================================

            if (request.FirstName.Length < 2)
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "First Name should contain minimum 2 characters."
                };
            }

            //=========================================
            // Last Name Validation
            //=========================================

            if (request.LastName.Length < 2)
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "Last Name should contain minimum 2 characters."
                };
            }

            //=========================================
            // Experience Validation
            //=========================================

            if (request.ExperienceYears < 0)
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "Experience cannot be negative."
                };
            }

            //=========================================
            // TODO
            // Duplicate Email Validation
            //=========================================

            //=========================================
            // TODO
            // Duplicate Phone Validation
            //=========================================

            var result =
                await _repo.RegisterTrainer(request);

            return new ServiceResponse
            {
                Success = true,
                Message = "Trainer Registered Successfully.",
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

    public async Task<object?> GetTrainerById(
        string trainerId)
    {
        return await _repo.GetTrainerById(trainerId);
    }

    public async Task<ServiceResponse> UpdateTrainer(
        string trainerId,
        UpdateTrainerRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(trainerId))
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "Trainer Id is required."
                };
            }

            var trainer =
                await _repo.GetTrainerById(trainerId);

            if (trainer == null)
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "Trainer Not Found."
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

            if (request.ExperienceYears < 0)
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "Experience cannot be negative."
                };
            }

            bool result =
                await _repo.UpdateTrainer(
                    trainerId,
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
                Message = "Trainer Updated Successfully."
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