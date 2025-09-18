using System.Text;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using FluentAssertions;
using Moq;
using Streetcode.BLL.Services.BlobStorageService;
using Xunit;

namespace Streetcode.XUnitTest.BLL.Services;

public class AzureBlobServiceTests
{
    private readonly Mock<BlobContainerClient> _mockContainerClient;
    private readonly Mock<BlobClient> _mockBlobClient;
    private readonly AzureBlobService _azureBlobService;

    public AzureBlobServiceTests()
    {
        _mockContainerClient = new Mock<BlobContainerClient>();
        _mockBlobClient = new Mock<BlobClient>();

        _mockContainerClient
            .Setup(x => x.GetBlobClient(It.IsAny<string>()))
            .Returns(_mockBlobClient.Object);

        _azureBlobService = new AzureBlobService(_mockContainerClient.Object);
    }

    [Fact]
    public void SaveFileInStorage_ValidBase64AndParameters_ShouldReturnHashedBlobName()
    {
        // Arrange
        var testData = "Hello World";
        var base64Data = Convert.ToBase64String(Encoding.UTF8.GetBytes(testData));
        var fileName = "test-file";
        var mimeType = "jpg";

        var mockResponse = new Mock<Response<BlobContentInfo>>();
        var blobContentInfo = BlobsModelFactory.BlobContentInfo(
            eTag: new ETag("etag"),
            lastModified: DateTimeOffset.UtcNow,
            contentHash: Array.Empty<byte>(),
            versionId: "version",
            encryptionKeySha256: "key",
            encryptionScope: "scope",
            blobSequenceNumber: 1);
        mockResponse.Setup(x => x.Value).Returns(blobContentInfo);

        _mockBlobClient
            .Setup(x => x.Upload(It.IsAny<Stream>(), It.IsAny<BlobUploadOptions>(), It.IsAny<CancellationToken>()))
            .Returns(mockResponse.Object);

        // Act
        var result = _azureBlobService.SaveFileInStorage(base64Data, fileName, mimeType);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().EndWith($".{mimeType}");

        _mockContainerClient.Verify(x => x.GetBlobClient(It.IsAny<string>()), Times.Once);
        _mockBlobClient.Verify(x => x.Upload(It.IsAny<Stream>(), It.IsAny<BlobUploadOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("jpg", "image/jpeg")]
    [InlineData("jpeg", "image/jpeg")]
    [InlineData("png", "image/png")]
    [InlineData("gif", "image/gif")]
    [InlineData("mp3", "audio/mpeg")]
    [InlineData("wav", "audio/wav")]
    [InlineData("pdf", "application/pdf")]
    [InlineData("unknown", "application/octet-stream")]
    public void SaveFileInStorage_DifferentMimeTypes_ShouldSetCorrectContentType(string mimeType, string expectedContentType)
    {
        // Arrange
        var testData = "Hello World";
        var base64Data = Convert.ToBase64String(Encoding.UTF8.GetBytes(testData));
        var fileName = "test-file";

        var mockResponse = new Mock<Response<BlobContentInfo>>();
        var blobContentInfo = BlobsModelFactory.BlobContentInfo(
            eTag: new ETag("etag"),
            lastModified: DateTimeOffset.UtcNow,
            contentHash: Array.Empty<byte>(),
            versionId: "version",
            encryptionKeySha256: "key",
            encryptionScope: "scope",
            blobSequenceNumber: 1);
        mockResponse.Setup(x => x.Value).Returns(blobContentInfo);

        BlobUploadOptions capturedOptions = null!;
        _mockBlobClient
            .Setup(x => x.Upload(It.IsAny<Stream>(), It.IsAny<BlobUploadOptions>(), It.IsAny<CancellationToken>()))
            .Callback<Stream, BlobUploadOptions, CancellationToken>((_, options, _) => capturedOptions = options)
            .Returns(mockResponse.Object);

        // Act
        _azureBlobService.SaveFileInStorage(base64Data, fileName, mimeType);

        // Assert
        capturedOptions.Should().NotBeNull();
        capturedOptions.HttpHeaders.ContentType.Should().Be(expectedContentType);
    }

    [Fact]
    public void FindFileInStorageAsMemoryStream_ExistingFile_ShouldReturnMemoryStream()
    {
        // Arrange
        var blobName = "test-blob.jpg";
        var testData = Encoding.UTF8.GetBytes("Test file content");
        var binaryData = BinaryData.FromBytes(testData);

        var mockResponse = new Mock<Response<BlobDownloadResult>>();
        var downloadResult = BlobsModelFactory.BlobDownloadResult(content: binaryData);
        mockResponse.Setup(x => x.Value).Returns(downloadResult);

        _mockBlobClient.Setup(x => x.Exists(It.IsAny<CancellationToken>())).Returns(Response.FromValue(true, Mock.Of<Response>()));
        _mockBlobClient.Setup(x => x.DownloadContent()).Returns(mockResponse.Object);

        // Act
        var result = _azureBlobService.FindFileInStorageAsMemoryStream(blobName);

        // Assert
        result.Should().NotBeNull();
        result.ToArray().Should().BeEquivalentTo(testData);

        _mockContainerClient.Verify(x => x.GetBlobClient(blobName), Times.Once);
        _mockBlobClient.Verify(x => x.Exists(It.IsAny<CancellationToken>()), Times.Once);
        _mockBlobClient.Verify(x => x.DownloadContent(), Times.Once);
    }

    [Fact]
    public void FindFileInStorageAsMemoryStream_NonExistingFile_ShouldThrowFileNotFoundException()
    {
        // Arrange
        var blobName = "non-existing-blob.jpg";

        _mockBlobClient.Setup(x => x.Exists(It.IsAny<CancellationToken>())).Returns(Response.FromValue(false, Mock.Of<Response>()));

        // Act & Assert
        var action = () => _azureBlobService.FindFileInStorageAsMemoryStream(blobName);

        action.Should().Throw<FileNotFoundException>()
            .WithMessage($"Blob with name {blobName} not found.");

        _mockBlobClient.Verify(x => x.DownloadContent(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void FindFileInStorageAsBase64_ExistingFile_ShouldReturnBase64String()
    {
        // Arrange
        const string blobName = "test-blob.jpg";
        var testData = "Test file content"u8.ToArray();
        var expectedBase64 = Convert.ToBase64String(testData);
        var binaryData = BinaryData.FromBytes(testData);

        var mockResponse = new Mock<Response<BlobDownloadResult>>();
        var downloadResult = BlobsModelFactory.BlobDownloadResult(content: binaryData);
        mockResponse.Setup(x => x.Value).Returns(downloadResult);

        _mockBlobClient.Setup(x => x.Exists(It.IsAny<CancellationToken>())).Returns(Response.FromValue(true, Mock.Of<Response>()));
        _mockBlobClient.Setup(x => x.DownloadContent()).Returns(mockResponse.Object);

        // Act
        var result = _azureBlobService.FindFileInStorageAsBase64(blobName);

        // Assert
        result.Should().Be(expectedBase64);

        _mockContainerClient.Verify(x => x.GetBlobClient(blobName), Times.Once);
        _mockBlobClient.Verify(x => x.Exists(It.IsAny<CancellationToken>()), Times.Once);
        _mockBlobClient.Verify(x => x.DownloadContent(), Times.Once);
    }

    [Fact]
    public void FindFileInStorageAsBase64_NonExistingFile_ShouldThrowFileNotFoundException()
    {
        // Arrange
        var blobName = "non-existing-blob.jpg";

        _mockBlobClient.Setup(x => x.Exists(It.IsAny<CancellationToken>())).Returns(Response.FromValue(false, Mock.Of<Response>()));

        // Act & Assert
        var action = () => _azureBlobService.FindFileInStorageAsBase64(blobName);

        action.Should().Throw<FileNotFoundException>()
            .WithMessage($"Blob with name {blobName} not found.");
    }

    [Fact]
    public void UpdateFileInStorage_ValidParameters_ShouldDeleteOldAndSaveNewFile()
    {
        // Arrange
        var previousBlobName = "old-blob.jpg";
        var newBlobName = "new-file";
        var base64Data = Convert.ToBase64String(Encoding.UTF8.GetBytes("New content"));
        var extension = "png";

        var mockDeleteResponse = new Mock<Response<bool>>();
        mockDeleteResponse.Setup(x => x.Value).Returns(true);

        var mockUploadResponse = new Mock<Response<BlobContentInfo>>();
        var blobContentInfo = BlobsModelFactory.BlobContentInfo(
            eTag: new ETag("etag"),
            lastModified: DateTimeOffset.UtcNow,
            contentHash: Array.Empty<byte>(),
            versionId: "version",
            encryptionKeySha256: "key",
            encryptionScope: "scope",
            blobSequenceNumber: 1);
        mockUploadResponse.Setup(x => x.Value).Returns(blobContentInfo);

        _mockBlobClient.Setup(x => x.DeleteIfExists(It.IsAny<DeleteSnapshotsOption>(), It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()))
            .Returns(mockDeleteResponse.Object);
        _mockBlobClient.Setup(x => x.Upload(It.IsAny<Stream>(), It.IsAny<BlobUploadOptions>(), It.IsAny<CancellationToken>()))
            .Returns(mockUploadResponse.Object);

        // Act
        var result = _azureBlobService.UpdateFileInStorage(previousBlobName, base64Data, newBlobName, extension);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().EndWith($".{extension}");

        // Verify delete was called for the previous blob
        _mockContainerClient.Verify(x => x.GetBlobClient(previousBlobName), Times.Once);
        _mockBlobClient.Verify(x => x.DeleteIfExists(It.IsAny<DeleteSnapshotsOption>(), It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()), Times.Once);

        // Verify upload was called for the new blob
        _mockBlobClient.Verify(x => x.Upload(It.IsAny<Stream>(), It.IsAny<BlobUploadOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void DeleteFileInStorage_ExistingFile_ShouldCallDeleteIfExists()
    {
        // Arrange
        var blobName = "test-blob.jpg";

        var mockResponse = new Mock<Response<bool>>();
        mockResponse.Setup(x => x.Value).Returns(true);

        _mockBlobClient.Setup(x => x.DeleteIfExists(It.IsAny<DeleteSnapshotsOption>(), It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()))
            .Returns(mockResponse.Object);

        // Act
        _azureBlobService.DeleteFileInStorage(blobName);

        // Assert
        _mockContainerClient.Verify(x => x.GetBlobClient(blobName), Times.Once);
        _mockBlobClient.Verify(x => x.DeleteIfExists(It.IsAny<DeleteSnapshotsOption>(), It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void DeleteFileInStorage_NonExistingFile_ShouldNotThrow()
    {
        // Arrange
        var blobName = "non-existing-blob.jpg";

        var mockResponse = new Mock<Response<bool>>();
        mockResponse.Setup(x => x.Value).Returns(false);

        _mockBlobClient.Setup(x => x.DeleteIfExists(It.IsAny<DeleteSnapshotsOption>(), It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()))
            .Returns(mockResponse.Object);

        // Act & Assert
        var action = () => _azureBlobService.DeleteFileInStorage(blobName);
        action.Should().NotThrow();

        _mockContainerClient.Verify(x => x.GetBlobClient(blobName), Times.Once);
        _mockBlobClient.Verify(x => x.DeleteIfExists(It.IsAny<DeleteSnapshotsOption>(), It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("test file.txt")]
    [InlineData("file:with:colons.jpg")]
    [InlineData("file.with.dots.png")]
    [InlineData("normal-file.pdf")]
    public void SaveFileInStorage_FileNameWithSpecialCharacters_ShouldReplaceSpecialCharacters(string fileName)
    {
        // Arrange
        var testData = "Hello World";
        var base64Data = Convert.ToBase64String(Encoding.UTF8.GetBytes(testData));
        var mimeType = "jpg";

        var mockResponse = new Mock<Response<BlobContentInfo>>();
        var blobContentInfo = BlobsModelFactory.BlobContentInfo(
            eTag: new ETag("etag"),
            lastModified: DateTimeOffset.UtcNow,
            contentHash: Array.Empty<byte>(),
            versionId: "version",
            encryptionKeySha256: "key",
            encryptionScope: "scope",
            blobSequenceNumber: 1);
        mockResponse.Setup(x => x.Value).Returns(blobContentInfo);

        string capturedBlobName = null!;
        _mockContainerClient
            .Setup(x => x.GetBlobClient(It.IsAny<string>()))
            .Callback<string>(name => capturedBlobName = name)
            .Returns(_mockBlobClient.Object);

        _mockBlobClient
            .Setup(x => x.Upload(It.IsAny<Stream>(), It.IsAny<BlobUploadOptions>(), It.IsAny<CancellationToken>()))
            .Returns(mockResponse.Object);

        // Act
        var result = _azureBlobService.SaveFileInStorage(base64Data, fileName, mimeType);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().EndWith($".{mimeType}");

        // The blob name should be hashed, but we can verify special characters were replaced
        capturedBlobName.Should().NotContain(" ");
        capturedBlobName.Should().NotContain(":");
        capturedBlobName[.. (capturedBlobName.Length - mimeType.Length - 1)] // Exclude extension
            .Should().NotContain(".");
    }

    [Fact]
    public void SaveFileInStorage_InvalidBase64_ShouldThrowFormatException()
    {
        // Arrange
        var base64Data = "invalid-base64-string";
        var fileName = "test-file";
        var mimeType = "jpg";

        // Act & Assert
        var action = () => _azureBlobService.SaveFileInStorage(base64Data, fileName, mimeType);
        action.Should().Throw<FormatException>();
    }

    [Fact]
    public void SaveFileInStorage_NullBase64_ShouldThrowArgumentNullException()
    {
        // Arrange
        string base64Data = null!;
        var fileName = "test-file";
        var mimeType = "jpg";

        // Act & Assert
        var action = () => _azureBlobService.SaveFileInStorage(base64Data, fileName, mimeType);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void SaveFileInStorage_LargeBinaryData_ShouldUploadSuccessfully()
    {
        // Arrange
        var largeData = new byte[1024 * 1024]; // 1MB of data
        Random.Shared.NextBytes(largeData);
        var base64Data = Convert.ToBase64String(largeData);
        var fileName = "large-file";
        var mimeType = "bin";

        var mockResponse = new Mock<Response<BlobContentInfo>>();
        var blobContentInfo = BlobsModelFactory.BlobContentInfo(
            eTag: new ETag("etag"),
            lastModified: DateTimeOffset.UtcNow,
            contentHash: Array.Empty<byte>(),
            versionId: "version",
            encryptionKeySha256: "key",
            encryptionScope: "scope",
            blobSequenceNumber: 1);
        mockResponse.Setup(x => x.Value).Returns(blobContentInfo);

        Stream capturedStream = null!;
        _mockBlobClient
            .Setup(x => x.Upload(It.IsAny<Stream>(), It.IsAny<BlobUploadOptions>(), It.IsAny<CancellationToken>()))
            .Callback<Stream, BlobUploadOptions, CancellationToken>((stream, _, _) => capturedStream = stream)
            .Returns(mockResponse.Object);

        // Act
        var result = _azureBlobService.SaveFileInStorage(base64Data, fileName, mimeType);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().EndWith($".{mimeType}");
        capturedStream.Should().NotBeNull();
    }

    [Fact]
    public void SaveFileInStorageWithName_ValidParameters_ShouldUploadWithCorrectNameAndExtension()
    {
        // Arrange
        var testData = "Hello World";
        var base64Data = Convert.ToBase64String(Encoding.UTF8.GetBytes(testData));
        var fileName = "custom-name";
        var extension = "png";

        var mockResponse = new Mock<Response<BlobContentInfo>>();
        var blobContentInfo = BlobsModelFactory.BlobContentInfo(
            eTag: new ETag("etag"),
            lastModified: DateTimeOffset.UtcNow,
            contentHash: Array.Empty<byte>(),
            versionId: "version",
            encryptionKeySha256: "key",
            encryptionScope: "scope",
            blobSequenceNumber: 1);
        mockResponse.Setup(x => x.Value).Returns(blobContentInfo);

        string capturedBlobName = null!;
        _mockContainerClient
            .Setup(x => x.GetBlobClient(It.IsAny<string>()))
            .Callback<string>(name => capturedBlobName = name)
            .Returns(_mockBlobClient.Object);

        BlobUploadOptions capturedOptions = null!;
        _mockBlobClient
            .Setup(x => x.Upload(It.IsAny<Stream>(), It.IsAny<BlobUploadOptions>(), It.IsAny<CancellationToken>()))
            .Callback<Stream, BlobUploadOptions, CancellationToken>((_, options, _) => capturedOptions = options)
            .Returns(mockResponse.Object);

        // Act
        _azureBlobService.SaveFileInStorageWithName(base64Data, fileName, extension);

        // Assert
        capturedBlobName.Should().Be($"{fileName}.{extension}");
        capturedOptions.Should().NotBeNull();
        capturedOptions.HttpHeaders.ContentType.Should().Be("image/png");
    }

    [Theory]
    [InlineData("jpg", "image/jpeg")]
    [InlineData("jpeg", "image/jpeg")]
    [InlineData("png", "image/png")]
    [InlineData("gif", "image/gif")]
    [InlineData("mp3", "audio/mpeg")]
    [InlineData("wav", "audio/wav")]
    [InlineData("pdf", "application/pdf")]
    [InlineData("unknown", "application/octet-stream")]
    public void SaveFileInStorageWithName_DifferentExtensions_ShouldSetCorrectContentType(string extension, string expectedContentType)
    {
        // Arrange
        var testData = "Hello World";
        var base64Data = Convert.ToBase64String(Encoding.UTF8.GetBytes(testData));
        var fileName = "custom-name";

        var mockResponse = new Mock<Response<BlobContentInfo>>();
        var blobContentInfo = BlobsModelFactory.BlobContentInfo(
            eTag: new ETag("etag"),
            lastModified: DateTimeOffset.UtcNow,
            contentHash: Array.Empty<byte>(),
            versionId: "version",
            encryptionKeySha256: "key",
            encryptionScope: "scope",
            blobSequenceNumber: 1);
        mockResponse.Setup(x => x.Value).Returns(blobContentInfo);

        BlobUploadOptions capturedOptions = null!;
        _mockBlobClient
            .Setup(x => x.Upload(It.IsAny<Stream>(), It.IsAny<BlobUploadOptions>(), It.IsAny<CancellationToken>()))
            .Callback<Stream, BlobUploadOptions, CancellationToken>((_, options, _) => capturedOptions = options)
            .Returns(mockResponse.Object);

        // Act
        _azureBlobService.SaveFileInStorageWithName(base64Data, fileName, extension);

        // Assert
        capturedOptions.Should().NotBeNull();
        capturedOptions.HttpHeaders.ContentType.Should().Be(expectedContentType);
    }

    [Fact]
    public void SaveFileInStorageWithName_InvalidBase64_ShouldThrowFormatException()
    {
        // Arrange
        var base64Data = "invalid-base64-string";
        var fileName = "custom-name";
        var extension = "jpg";

        // Act & Assert
        var action = () => _azureBlobService.SaveFileInStorageWithName(base64Data, fileName, extension);
        action.Should().Throw<FormatException>();
    }

    [Fact]
    public void SaveFileInStorageWithName_NullBase64_ShouldThrowArgumentNullException()
    {
        // Arrange
        string base64Data = null!;
        var fileName = "custom-name";
        var extension = "jpg";

        // Act & Assert
        var action = () => _azureBlobService.SaveFileInStorageWithName(base64Data, fileName, extension);
        action.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData("file with spaces.txt")]
    [InlineData("file:with:colons.jpg")]
    [InlineData("file.with.dots.png")]
    [InlineData("normal-file.pdf")]
    public void SaveFileInStorageWithName_FileNameWithSpecialCharacters_ShouldPreserveNameAndExtension(string fileName)
    {
        // Arrange
        var testData = "Hello World";
        var base64Data = Convert.ToBase64String(Encoding.UTF8.GetBytes(testData));
        var extension = "jpg";

        var mockResponse = new Mock<Response<BlobContentInfo>>();
        var blobContentInfo = BlobsModelFactory.BlobContentInfo(
            eTag: new ETag("etag"),
            lastModified: DateTimeOffset.UtcNow,
            contentHash: Array.Empty<byte>(),
            versionId: "version",
            encryptionKeySha256: "key",
            encryptionScope: "scope",
            blobSequenceNumber: 1);
        mockResponse.Setup(x => x.Value).Returns(blobContentInfo);

        string capturedBlobName = null!;
        _mockContainerClient
            .Setup(x => x.GetBlobClient(It.IsAny<string>()))
            .Callback<string>(name => capturedBlobName = name)
            .Returns(_mockBlobClient.Object);

        _mockBlobClient
            .Setup(x => x.Upload(It.IsAny<Stream>(), It.IsAny<BlobUploadOptions>(), It.IsAny<CancellationToken>()))
            .Returns(mockResponse.Object);

        // Act
        _azureBlobService.SaveFileInStorageWithName(base64Data, fileName, extension);

        // Assert
        capturedBlobName.Should().Be($"{fileName}.{extension}");
    }
}
