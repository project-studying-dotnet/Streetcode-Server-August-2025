using AutoMapper;
using Moq;
using Streetcode.BLL.DTO.Streetcode;
using Streetcode.BLL.DTO.Streetcode.Create;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.Streetcode.Create;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;
using Streetcode.BLL.DTO.AdditionalContent.Tag;
using Streetcode.BLL.DTO.Media.Images;

namespace Streetcode.XUnitTest.BLL.MediatR.StreetCode.StreetCode.Create;

public class StreetcodeCreateHandlerTest
{
    private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly StreetcodeCreateHandler _handler;

    public StreetcodeCreateHandlerTest()
    {
        _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILoggerService>();
        _handler = new StreetcodeCreateHandler(
            _mockRepositoryWrapper.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_SaveStreetcodeFails_ShouldReturnFailure()
    {
        // Arrange
        var request = CreateValidRequest();
        var streetcodeEntity = CreateStreetcodeEntity();

        _mockMapper.Setup(m => m.Map<StreetcodeContent>(request.NewStreetcode))
            .Returns(streetcodeEntity);
        _mockRepositoryWrapper.Setup(r => r.StreetcodeRepository.Create(streetcodeEntity));
        _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(0);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        _mockLogger.Verify(l => l.LogError(request, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Handle_EmptyImageIds_ShouldReturnFailure()
    {
        // Arrange
        var request = CreateRequestWithEmptyImages();
        var streetcodeEntity = CreateStreetcodeEntity();

        SetupMocksForInitialSave(request, streetcodeEntity);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Contains("ImagesDetails cannot be empty", result.Errors[0].Message);
    }

    [Fact]
    public async Task Handle_EmptyTags_ShouldReturnFailure()
    {
        // Arrange
        var request = CreateRequestWithEmptyTags();
        var streetcodeEntity = CreateStreetcodeEntity();

        SetupMocksForInitialSave(request, streetcodeEntity);
        SetupImagesForSuccess(request);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Contains("Object reference not set to an instance of an object.", result.Errors[0].Message);
    }

    [Fact]
    public async Task Handle_EmptyImagesDetails_ShouldReturnFailure()
    {
        // Arrange
        var request = CreateRequestWithEmptyImagesDetails();
        var streetcodeEntity = CreateStreetcodeEntity();

        SetupMocksForInitialSave(request, streetcodeEntity);
        SetupImagesForSuccess(request);
        SetupTagsForSuccess(request);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Contains("ImagesDetails cannot be empty", result.Errors[0].Message);
    }

    [Fact]
    public async Task Handle_SaveRelationshipsFails_ShouldReturnFailure()
    {
        // Arrange
        var request = CreateValidRequest();
        var streetcodeEntity = CreateStreetcodeEntity();

        SetupMocksForInitialSave(request, streetcodeEntity);
        SetupImagesForSuccess(request);
        SetupTagsForSuccess(request);
        SetupImagesDetailsForSuccess(request);

        _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(0);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Contains("Object reference not set to an instance of an object.", result.Errors[0].Message);
    }

    [Fact]
    public async Task Handle_ExceptionThrown_ShouldReturnFailure()
    {
        // Arrange
        var request = CreateValidRequest();

        _mockMapper.Setup(m => m.Map<StreetcodeContent>(request.NewStreetcode))
            .Throws(new Exception("Test exception"));

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Contains("Test exception", result.Errors[0].Message);
        _mockLogger.Verify(l => l.LogError(request, It.IsAny<string>()), Times.Once);
    }

    private StreetcodeCreateCommand CreateValidRequest()
    {
        return new StreetcodeCreateCommand(new StreetcodeCreateDTO
        {
            Title = "Test Streetcode",
            TransliterationUrl = "test-streetcode",
            Index = 1,
            Teaser = "Test teaser",
            DateString = "2024",
            ImagesDetails = new List<ImageDetailsDto>
            {
                new ImageDetailsDto { ImageId = 1, Alt = "Test image" }
            },
            Tags = new List<StreetcodeTagDTO>
            {
                new StreetcodeTagDTO { Title = "Test Tag" }
            }
        });
    }

    private StreetcodeCreateCommand CreateRequestWithEmptyImages()
    {
        var request = CreateValidRequest();
        request.NewStreetcode.ImagesDetails = new List<ImageDetailsDto>();
        return request;
    }

    private StreetcodeCreateCommand CreateRequestWithEmptyTags()
    {
        var request = CreateValidRequest();
        request.NewStreetcode.Tags = new List<StreetcodeTagDTO>();
        return request;
    }

    private StreetcodeCreateCommand CreateRequestWithEmptyImagesDetails()
    {
        var request = CreateValidRequest();
        request.NewStreetcode.ImagesDetails = new List<ImageDetailsDto>();
        return request;
    }

    private StreetcodeContent CreateStreetcodeEntity()
    {
        return new StreetcodeContent
        {
            Id = 1,
            Title = "Test Streetcode",
            TransliterationUrl = "test-streetcode",
            Index = 1
        };
    }

    private StreetcodeDTO CreateStreetcodeDTO()
    {
        return new StreetcodeDTO
        {
            Id = 1,
            Title = "Test Streetcode",
            TransliterationUrl = "test-streetcode",
            Index = 1
        };
    }

    private void SetupMocksForSuccess(StreetcodeCreateCommand request, StreetcodeContent entity, StreetcodeDTO dto)
    {
        SetupMocksForInitialSave(request, entity);
        SetupImagesForSuccess(request);
        SetupTagsForSuccess(request);
        SetupImagesDetailsForSuccess(request);

        _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        _mockMapper.Setup(m => m.Map<StreetcodeDTO>(entity))
            .Returns(dto);
    }

    private void SetupMocksForInitialSave(StreetcodeCreateCommand request, StreetcodeContent entity)
    {
        _mockMapper.Setup(m => m.Map<StreetcodeContent>(request.NewStreetcode))
            .Returns(entity);

        _mockRepositoryWrapper.Setup(r => r.StreetcodeRepository.Create(entity));
        _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);
    }

    private void SetupImagesForSuccess(StreetcodeCreateCommand request)
    {
        _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);
    }

    private void SetupTagsForSuccess(StreetcodeCreateCommand request)
    {
        _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);
    }

    private void SetupImagesDetailsForSuccess(StreetcodeCreateCommand request)
    {
        _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);
    }

    private void VerifySuccessCalls(StreetcodeCreateCommand request, StreetcodeContent entity)
    {
        _mockMapper.Verify(m => m.Map<StreetcodeContent>(request.NewStreetcode), Times.Once);
        _mockRepositoryWrapper.Verify(r => r.StreetcodeRepository.Create(entity), Times.Once);
        _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.AtLeast(2));
        _mockMapper.Verify(m => m.Map<StreetcodeDTO>(entity), Times.Once);
        _mockLogger.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Once);
    }
}