using LMS.Application.Common;
using LMS.Application.Features.Admin.DTOs;

namespace LMS.Application.Features.Auth.Services.Admin;

public interface IAdminService
{
    Task<ServiceResponse> RegisterAdmin(
        RegisterAdminRequest request);

    Task<object?> GetAdminById(
        string adminId);

    Task<ServiceResponse> UpdateAdmin(
        string adminId,
        UpdateAdminRequest request);
}

public interface IAdminRepository
{
    Task<object> RegisterAdmin(
        RegisterAdminRequest request);

    Task<object?> GetAdminById(
        string adminId);

    Task<bool> UpdateAdmin(
        string adminId,
        UpdateAdminRequest request);
}