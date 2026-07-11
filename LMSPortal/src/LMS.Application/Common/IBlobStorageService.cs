using Microsoft.AspNetCore.Http;

namespace LMS.Application.Common;

public interface IBlobStorageService
{
    Task<string> UploadFileAsync(IFormFile file, string folder);

    Task<string> UploadStreamAsync(
        Stream stream,
        string containerName,
        string blobName,
        string contentType);

    Task<byte[]> DownloadAsync(string containerName, string blobName);
}
