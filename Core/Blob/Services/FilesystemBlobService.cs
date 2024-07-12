using Lens.Core.Blob.Models;
using Lens.Core.Lib.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Lens.Core.Blob.Services;

public class FilesystemBlobService : BaseService<FilesystemBlobService>, IBlobService
{
    private readonly BlobSettings _blobServiceSettings;

    public FilesystemBlobService(
        IApplicationService<FilesystemBlobService> applicationService, 
        IConfiguration configuration) : base(applicationService)
    {
        _blobServiceSettings = configuration.GetSection(nameof(BlobSettings)).Get<BlobSettings>();
    }

    public async Task<bool> DeleteBlob(string relativePathAndName)
    {
        var root = _blobServiceSettings.ContainerPath ?? string.Empty;
        var path = Path.Combine(root, relativePathAndName);
        
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        else
        {
            // log, but don't throw error if a blob couldn't be find at the given path.
            ApplicationService.Logger.LogWarning($"No file found at the given path '{relativePathAndName}'.");
        }

        return await Task.FromResult(true);
    }

    public async Task<Stream> Download(string relativePathAndName)
    {
        var root = _blobServiceSettings.ContainerPath ?? string.Empty;
        var path = Path.Combine(root, relativePathAndName);
        
        return await Task.FromResult(File.OpenRead(path));
    }

    public async Task<string[]> GetBlobs()
    {
        var root = _blobServiceSettings.ContainerPath ?? string.Empty;
        var path = Path.Combine(root);
        string[] fileEntries = Directory.GetFiles(path);

        return await Task.FromResult(fileEntries);
    }

    public async Task<string> GetBlobUrl(string relativePathAndName)
    {
        var root = _blobServiceSettings.ContainerPath ?? string.Empty;
        var path = Path.Combine(root, relativePathAndName);
        
        return File.Exists(path) 
            ? await Task.FromResult(path.Replace(root, "~").Replace("\\", "/")) 
            : await Task.FromResult(string.Empty);
    }

    public async Task<BlobMetadataModel> Upload(string relativePathAndName, Stream stream, Dictionary<string, string>? additionalInformation = null)
    {
        var extension = Path.GetExtension(relativePathAndName);
        var newFileName = $"{Guid.NewGuid()}{extension}";
        var relativePath = Path.GetDirectoryName(relativePathAndName) ?? string.Empty;
        var folderPath = Path.Combine(_blobServiceSettings.ContainerPath ?? string.Empty, relativePath);

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
        var filePath = Path.Combine(folderPath, newFileName);
        
        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await stream.CopyToAsync(fileStream);
        }

        return new BlobMetadataModel 
        { 
            RelativePathAndName = Path.Combine(relativePath, newFileName),
            FullPathAndName = filePath 
        };
    }

    public Task<BlobDownloadResultModel> DownloadWithMetadata(string relativePathAndName)
    {
        throw new NotImplementedException();
    }

    public Task MoveBlobWithinContainer(string sourceRelativePathAndName, string targetRelativePathAndName)
    {
        throw new NotImplementedException();
    }
}
