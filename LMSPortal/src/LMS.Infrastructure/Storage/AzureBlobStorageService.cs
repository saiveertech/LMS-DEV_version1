using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using LMS.Application.Common;
using LMS.Infrastructure.Configurations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace LMS.Infrastructure.Storage;

public class AzureBlobStorageService : IBlobStorageService
{
    private readonly AzureStorageSettings _settings;

    public AzureBlobStorageService(IOptions<AzureStorageSettings> options)
    {
        _settings = options.Value;
    }

    public async Task<string> UploadFileAsync(IFormFile file, string folder)
    {
        var containerClient = new BlobContainerClient(
            _settings.ConnectionString,
            _settings.ContainerName);

        await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

        var blobName = $"{folder}/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

        var blobClient = containerClient.GetBlobClient(blobName);

        await using var stream = file.OpenReadStream();

        await blobClient.UploadAsync(
            stream,
            new BlobHttpHeaders { ContentType = file.ContentType });

        if (!blobClient.CanGenerateSasUri)
        {
            throw new InvalidOperationException(
                "Cannot generate a SAS URI for the uploaded blob. The storage connection string must include an account key.");
        }

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = containerClient.Name,
            BlobName = blobClient.Name,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.AddYears(10)
        };

        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        var sasUri = blobClient.GenerateSasUri(sasBuilder);

        return sasUri.ToString();
    }
}
