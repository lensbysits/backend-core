namespace Lens.Core.Blob;

public class BlobSettings
{
    public string? ConnectionString { get; set; }

    public string? LocationName { get; set; }

    public string? ContainerPath { get; set; }

        public string Provider { get; set; } = BlobProvider_FileSystem;

        public const string BlobProvider_AzureStorage = "azure";
        public const string BlobProvider_FileSystem = "filesystem";
}
