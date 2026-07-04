using System.ComponentModel.DataAnnotations;
using LMS.Application.Common;
using LMS.Application.Features.Admin.DTOs;
namespace LMS.Application.Features.Auth.Services.Admin;

public class AdminService : IAdminService
{
    private readonly IAdminRepository _repo;

    public AdminService(IAdminRepository repo)
    {
        _repo = repo;
    }

    public async Task<ServiceResponse> RegisterAdmin(RegisterAdminRequest request)
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

            if (request.ExperienceYears < 0)
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "Experience cannot be negative."
                };
            }

            var result = await _repo.RegisterAdmin(request);

            return new ServiceResponse
            {
                Success = true,
                Message = "Admin Registered Successfully.",
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

    public async Task<object?> GetAdminById(string? adminId = null)
    {
        return await _repo.GetAdminById(adminId);
    }

    
public async Task<ServiceResponse> UpdateAdmin(
    string adminId,
    UpdateAdminRequest request)
{
    try
    {
        if (string.IsNullOrWhiteSpace(adminId))
        {
            return new ServiceResponse
            {
                Success = false,
                Message = "Admin Id is required."
            };
        }

        var admin =
            await _repo.GetAdminById(adminId);

        if(admin==null)
        {
            return new ServiceResponse
            {
                Success=false,
                Message="Admin Not Found."
            };
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            if (!new EmailAddressAttribute().IsValid(request.Email))
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "Invalid Email."
                };
            }
        }

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            if(request.PhoneNumber.Length!=10)
            {
                return new ServiceResponse
                {
                    Success=false,
                    Message="Phone Number should contain 10 digits."
                };
            }

            if(!request.PhoneNumber.All(char.IsDigit))
            {
                return new ServiceResponse
                {
                    Success=false,
                    Message="Phone Number should contain only digits."
                };
            }
        }

        if(request.ExperienceYears<0)
        {
            return new ServiceResponse
            {
                Success=false,
                Message="Experience cannot be negative."
            };
        }

        bool result =
            await _repo.UpdateAdmin(
                adminId,
                request);

        if(!result)
        {
            return new ServiceResponse
            {
                Success=false,
                Message="Update Failed."
            };
        }

        return new ServiceResponse
        {
            Success=true,
            Message="Admin Updated Successfully."
        };
    }
    catch(Exception ex)
    {
        return new ServiceResponse
        {
            Success=false,
            Message=ex.Message
        };
    }
}

}