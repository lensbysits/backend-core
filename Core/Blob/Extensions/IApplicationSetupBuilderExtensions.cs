using Lens.Core.Blob.Services;
using Lens.Core.Lib.Builders;
using Lens.Core.Lib.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lens.Core.Blob;

public static class IApplicationSetupBuilderExtensions
{
    public static IApplicationSetupBuilder AddBlobService(this IApplicationSetupBuilder builder, 
        BlobImplementationType blobImplementationType)
    {
        var blobSettings = builder.Configuration.GetSection(nameof(BlobSettings)).Get<BlobSettings>();
        if (blobSettings == null)
        {
            throw new NotFoundException($"Missing '{nameof(BlobSettings)}' configuration section.");
        }
        if (string.IsNullOrEmpty(blobSettings.ContainerPath))
        {
            throw new NotFoundException($"Missing value for '{nameof(BlobSettings.ContainerPath)}'");
        }

        if (blobImplementationType == BlobImplementationType.AzureStorage)
        {
            if (string.IsNullOrEmpty(blobSettings.ConnectionString) && string.IsNullOrEmpty(blobSettings.LocationName))
            {
                throw new NotFoundException($"Missing value for '{nameof(BlobSettings.ConnectionString)}' or '{nameof(BlobSettings.LocationName)}'");
            }
        }

        switch (blobImplementationType)
        {
            case BlobImplementationType.Filesystem:
                builder.Services.AddScoped<IBlobService, FilesystemBlobService>();
                break;
            case BlobImplementationType.AzureStorage:
                builder.Services.AddScoped<IBlobService, AzureStorageBlobService>();
                break;
            default:
                throw new NotFoundException("Unknown blob implementation type");
        }

        return builder;
    }
}
