using LMS.Application.Common;
using LMS.Application.Features.Auth.DTOs;

namespace LMS.Application.Features.Auth.Services.Trainer;

public interface ITrainerService
{
    Task<ServiceResponse> RegisterTrainer(RegisterTrainerRequest request);

    Task<object?> GetTrainerById(string trainerId);

    Task<ServiceResponse> UpdateTrainer(
        string trainerId,
        UpdateTrainerRequest request);
}

public interface ITrainerRepository
{
    Task<object> RegisterTrainer(RegisterTrainerRequest request);

    Task<object?> GetTrainerById(string trainerId);

    Task<bool> UpdateTrainer(
        string trainerId,
        UpdateTrainerRequest request);
}