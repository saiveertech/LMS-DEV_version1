using Microsoft.AspNetCore.Http;

namespace LMS.Application.Common;

public interface IBlobStorageService
{
    Task<string> UploadFileAsync(IFormFile file, string folder);
}
