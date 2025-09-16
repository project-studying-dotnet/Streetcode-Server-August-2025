using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Services.BlobStorageService;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.Factories.BlobStorage;

public class BlobServiceFactory : IBlobServiceFactory
{
    private readonly IOptions<BlobEnvironmentVariables> _blobOptions;
    private readonly IOptions<AzureBlobEnvironmentVariables> _azureBlobOptions;
    private readonly IRepositoryWrapper _repositoryWrapper;

    public BlobServiceFactory(
        IOptions<BlobEnvironmentVariables> blobOptions,
        IOptions<AzureBlobEnvironmentVariables> azureBlobOptions,
        IRepositoryWrapper repositoryWrapper)
    {
        _blobOptions = blobOptions;
        _azureBlobOptions = azureBlobOptions;
        _repositoryWrapper = repositoryWrapper;
    }

    public IBlobService CreateBlobService()
    {
        var storageType = _blobOptions.Value.StorageType?.ToLower() ?? "local";

        return storageType switch
        {
            "azure" => CreateAzureBlobService(),
            _ => new BlobService(_blobOptions, _repositoryWrapper),
        };
    }

    private IBlobService CreateAzureBlobService()
    {
        var options = _azureBlobOptions.Value;

        var blobServiceClient = new BlobServiceClient(options.ConnectionString);
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(options.ContainerName);

        return new AzureBlobService(blobContainerClient);
    }
}
