using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Streetcode.BLL.Factories.BlobStorage;
using Streetcode.BLL.Services.BlobStorageService;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.Factories;

public class BlobServiceFactoryTests
{
    private readonly Mock<IOptions<BlobEnvironmentVariables>> _mockBlobOptions;
    private readonly Mock<IOptions<AzureBlobEnvironmentVariables>> _mockAzureBlobOptions;
    private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;

    public BlobServiceFactoryTests()
    {
        _mockBlobOptions = new Mock<IOptions<BlobEnvironmentVariables>>();
        _mockAzureBlobOptions = new Mock<IOptions<AzureBlobEnvironmentVariables>>();
        _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
    }

    [Fact]
    public void CreateBlobService_WithValidAzureConfig_ReturnsAzureBlobService()
    {
        // Arrange
        var blobConfig = new BlobEnvironmentVariables
        {
            BlobStorePath = "../../BlobStorageFolder/",
            BlobStoreKey = "test-key",
            StorageType = "Azure"
        };

        var azureConfig = new AzureBlobEnvironmentVariables
        {
            ConnectionString = "DefaultEndpointsProtocol=https;AccountName=test;AccountKey=dGVzdA==;EndpointSuffix=core.windows.net",
            ContainerName = "test-container"
        };

        _mockBlobOptions.Setup(x => x.Value).Returns(blobConfig);
        _mockAzureBlobOptions.Setup(x => x.Value).Returns(azureConfig);

        var factory = new BlobServiceFactory(_mockBlobOptions.Object, _mockAzureBlobOptions.Object, _mockRepositoryWrapper.Object);

        // Act
        var result = factory.CreateBlobService();

        // Assert
        result.Should().BeOfType<AzureBlobService>();
    }

    [Fact]
    public void CreateBlobService_WithEmptyConnectionString_ReturnsLocalBlobService()
    {
        // Arrange
        var blobConfig = new BlobEnvironmentVariables
        {
            BlobStorePath = "../../BlobStorageFolder/",
            BlobStoreKey = "test-key",
        };

        var azureConfig = new AzureBlobEnvironmentVariables
        {
            ConnectionString = "", // порожня
            ContainerName = "test-container"
        };

        _mockBlobOptions.Setup(x => x.Value).Returns(blobConfig);
        _mockAzureBlobOptions.Setup(x => x.Value).Returns(azureConfig);

        var factory = new BlobServiceFactory(_mockBlobOptions.Object, _mockAzureBlobOptions.Object, _mockRepositoryWrapper.Object);

        // Act
        var result = factory.CreateBlobService();

        // Assert
        result.Should().BeOfType<BlobService>();
    }

    [Fact]
    public void CreateBlobService_WithEmptyContainerName_ReturnsLocalBlobService()
    {
        // Arrange
        var blobConfig = new BlobEnvironmentVariables
        {
            BlobStorePath = "../../BlobStorageFolder/",
            BlobStoreKey = "test-key"
        };

        var azureConfig = new AzureBlobEnvironmentVariables
        {
            ConnectionString = "DefaultEndpointsProtocol=https;AccountName=test;AccountKey=dGVzdA==;EndpointSuffix=core.windows.net",
            ContainerName = "" // порожня
        };

        _mockBlobOptions.Setup(x => x.Value).Returns(blobConfig);
        _mockAzureBlobOptions.Setup(x => x.Value).Returns(azureConfig);

        var factory = new BlobServiceFactory(_mockBlobOptions.Object, _mockAzureBlobOptions.Object, _mockRepositoryWrapper.Object);

        // Act
        var result = factory.CreateBlobService();

        // Assert
        result.Should().BeOfType<BlobService>();
    }

    [Fact]
    public void CreateBlobService_WithNullAzureConfig_ReturnsLocalBlobService()
    {
        // Arrange
        var blobConfig = new BlobEnvironmentVariables
        {
            BlobStorePath = "../../BlobStorageFolder/",
            BlobStoreKey = "test-key"
        };

        _mockBlobOptions.Setup(x => x.Value).Returns(blobConfig);
        _mockAzureBlobOptions.Setup(x => x.Value).Returns((AzureBlobEnvironmentVariables)null);

        var factory = new BlobServiceFactory(_mockBlobOptions.Object, _mockAzureBlobOptions.Object, _mockRepositoryWrapper.Object);

        // Act
        var result = factory.CreateBlobService();

        // Assert
        result.Should().BeOfType<BlobService>();
    }

    [Theory]
    [InlineData(null, "container")]
    [InlineData("", "container")]
    [InlineData("  ", "container")]
    [InlineData("connection-string", null)]
    [InlineData("connection-string", "")]
    [InlineData("connection-string", "  ")]
    [InlineData(null, null)]
    [InlineData("", "")]
    public void CreateBlobService_WithInvalidAzureConfig_ReturnsLocalBlobService(string connectionString, string containerName)
    {
        // Arrange
        var blobConfig = new BlobEnvironmentVariables
        {
            BlobStorePath = "../../BlobStorageFolder/",
            BlobStoreKey = "test-key"
        };

        var azureConfig = new AzureBlobEnvironmentVariables
        {
            ConnectionString = connectionString,
            ContainerName = containerName
        };

        _mockBlobOptions.Setup(x => x.Value).Returns(blobConfig);
        _mockAzureBlobOptions.Setup(x => x.Value).Returns(azureConfig);

        var factory = new BlobServiceFactory(_mockBlobOptions.Object, _mockAzureBlobOptions.Object, _mockRepositoryWrapper.Object);

        // Act
        var result = factory.CreateBlobService();

        // Assert
        result.Should().BeOfType<BlobService>();
    }
}
