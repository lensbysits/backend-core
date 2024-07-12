using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Lens.Core.Blob.Models;
using Lens.Core.Lib.Services;
using Microsoft.Extensions.Configuration;

namespace Lens.Core.Blob.Services;

public class AzureStorageBlobService : BaseService<AzureStorageBlobService>, IBlobService
{
    private readonly BlobSettings _blobServiceSettings;
    private readonly BlobContainerClient blobcontainerClient;

    public AzureStorageBlobService(
        IApplicationService<AzureStorageBlobService> applicationService,
        IConfiguration configuration, BlobServiceClient blobServiceClient) : base(applicationService)
    {
        _blobServiceSettings = configuration.GetSection(nameof(BlobSettings)).Get<BlobSettings>();
        this.blobcontainerClient = blobServiceClient.GetBlobContainerClient(_blobServiceSettings.ContainerPath);
    }

    public async Task<bool> DeleteBlob(string relativePathAndName)
    {
        BlobClient blobClient = blobcontainerClient.GetBlobClient(relativePathAndName);

        return (await blobClient.DeleteIfExistsAsync()).Value;
    }

    public async Task<Stream> Download(string relativePathAndName)
    {
        BlobClient blobClient = blobcontainerClient.GetBlobClient(relativePathAndName);

        var fStream = new MemoryStream();
        await blobClient.DownloadToAsync(fStream);

        return fStream;
    }

    public async Task<BlobDownloadResultModel> DownloadWithMetadata(string relativePathAndName)
    {
        BlobClient blobClient = blobcontainerClient.GetBlobClient(relativePathAndName);

        var fStream = new MemoryStream();
        var response = await blobClient.DownloadToAsync(fStream);

        return new BlobDownloadResultModel(
                    fStream, 
                    response.Headers.ContentType!, 
                    response.Headers.ContentLength);
    }

    public async Task<string[]> GetBlobs()
    {
        var values = new List<string>();
        await foreach (BlobItem blob in blobcontainerClient.GetBlobsAsync())
        {
            values.Add(blob.Name);
        }

        return values.ToArray();
    }

    public Task<string> GetBlobUrl(string relativePathAndName)
    {
        BlobClient blobClient = blobcontainerClient.GetBlobClient(relativePathAndName);
        return Task.FromResult(blobClient.Uri.AbsoluteUri.ToString());
    }

    public async Task<BlobMetadataModel> Upload(string relativePathAndName, Stream stream, Dictionary<string, string>? additionalInformation = null)
    {
        BlobClient blobClient = blobcontainerClient.GetBlobClient(relativePathAndName);
        var headers = new BlobHttpHeaders();
        if (additionalInformation?.ContainsKey("Content-Type") ?? false)
        {
            headers.ContentType = additionalInformation["Content-Type"];
        }

        var uploadInfo = await blobClient.UploadAsync(stream, headers);

        var blobMetadata = new BlobMetadataModel()
        {
            RelativePathAndName = relativePathAndName,
            FullPathAndName = blobClient.Uri.AbsoluteUri
        };

        return blobMetadata;
    }

    public Task MoveBlobWithinContainer(string sourceRelativePathAndName, string targetRelativePathAndName)
    {
        var containerPath = $"/{_blobServiceSettings.ContainerPath}";
        if (sourceRelativePathAndName.StartsWith(containerPath))
        {
            sourceRelativePathAndName = sourceRelativePathAndName[containerPath.Length..];
        }

        if (targetRelativePathAndName.StartsWith(containerPath))
        {
            targetRelativePathAndName = targetRelativePathAndName[containerPath.Length..];
        }


        BlobClient sourceBlobClient = blobcontainerClient.GetBlobClient(sourceRelativePathAndName);
        BlobClient targetBlobClient = blobcontainerClient.GetBlobClient(targetRelativePathAndName);
        var result = targetBlobClient.StartCopyFromUri(sourceBlobClient.Uri);
        return sourceBlobClient.DeleteAsync();
    }
}