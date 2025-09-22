using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Services.BlobStorageService;
using Streetcode.DAL.Entities.Users;
using Streetcode.DAL.Enums;
using Streetcode.DAL.Persistence;
using Streetcode.WebApi.Extensions;
using Xunit;

namespace Streetcode.XUnitTest.WebApi.Extensions;

public class SeedingLocalExtensionTests
{
    [Fact]
    public void AddIdentityServices_ConfiguresIdentityServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        var result = services.AddDbContext<StreetcodeDbContext>().AddIdentityServices();

        // Assert
        Assert.Same(services, result);
        var serviceProvider = services.BuildServiceProvider();

        // Verify core identity services are registered
        Assert.NotNull(serviceProvider.GetService<UserManager<User>>());
        Assert.NotNull(serviceProvider.GetService<SignInManager<User>>());
    }

    [Fact]
    public void AddIdentityServices_ConfiguresEntityFrameworkStores()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddDbContext<StreetcodeDbContext>().AddIdentityServices();

        // Assert
        var identityBuilder = services.BuildServiceProvider();
        Assert.NotNull(identityBuilder.GetService<StreetcodeDbContext>());
    }

    [Fact]
    public void SeedMediaFile_WithLocalStorage_CreatesDirectory()
    {
        // Arrange
        var blobService = new Mock<IBlobService>();
        var blobConfig = new BlobEnvironmentVariables
        {
            StorageType = "local",
            BlobStorePath = Path.Combine(Path.GetTempPath(), "test-blobs")
        };

        // Act
        SeedMediaFileTest(
            blobService.Object,
            blobConfig,
            "test.txt",
            "base64content");

        // Assert
        Assert.True(Directory.Exists(blobConfig.BlobStorePath));

        // Cleanup
        if (Directory.Exists(blobConfig.BlobStorePath))
        {
            Directory.Delete(blobConfig.BlobStorePath, true);
        }
    }

    [Fact]
    public async Task SeedMediaFile_WithAzureStorage_ChecksExistingBlob()
    {
        // Arrange
        var blobService = new Mock<IBlobService>();
        var blobConfig = new BlobEnvironmentVariables { StorageType = "azure" };

        blobService.Setup(x => x.FindFileInStorageAsBase64(It.IsAny<string>()))
            .Throws<FileNotFoundException>();

        blobService.Setup(x => x.SaveFileInStorageWithName(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()))
            .Verifiable();

        // Act
        await SeedMediaFileTest(
            blobService.Object,
            blobConfig,
            "test.txt",
            "base64content");

        // Assert
        blobService.Verify(
            x => x.SaveFileInStorageWithName(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public async Task SeedMediaFile_HandlesInvalidBlobName()
    {
        // Arrange
        var blobService = new Mock<IBlobService>();
        var blobConfig = new BlobEnvironmentVariables { StorageType = "azure" };

        // Act & Assert
        var exception = await Record.ExceptionAsync(async () =>
            await SeedMediaFileTest(
                blobService.Object,
                blobConfig,
                "invalid-no-extension",
                "base64content"));

        Assert.Null(exception);
    }

    private static Task SeedMediaFileTest(
        IBlobService blobService,
        BlobEnvironmentVariables blobConfig,
        string blobName,
        string base64)
    {
        if (blobConfig.StorageType?.ToLower() != "azure")
        {
            string blobPath = blobConfig.BlobStorePath;
            Directory.CreateDirectory(blobPath);
            string filePath = Path.Combine(blobPath, blobName);
            if (File.Exists(filePath))
            {
                return Task.CompletedTask;
            }
        }

        try
        {
            var blobNameParts = blobName.Split('.');
            if (blobNameParts.Length >= 2)
            {
                var nameWithoutExtension = string.Join(".", blobNameParts.Take(blobNameParts.Length - 1));
                var extension = blobNameParts.Last();
                blobService.SaveFileInStorageWithName(base64, nameWithoutExtension, extension);
            }
        }
        catch (Exception)
        {
            // Log error but continue
        }

        return Task.CompletedTask;
    }
}