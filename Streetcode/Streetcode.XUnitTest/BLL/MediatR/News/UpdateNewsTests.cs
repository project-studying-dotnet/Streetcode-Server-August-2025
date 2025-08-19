using AutoMapper;
using FluentAssertions;
using Moq;
using Repositories.Interfaces;
using Streetcode.BLL.DTO.Media.Images;
using Streetcode.BLL.DTO.News;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Newss.Update;
using Streetcode.DAL.Entities.Media.Images;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Repositories.Interfaces.Newss;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.News;

public class UpdateNewsTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IBlobService> _mockBlobService;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly UpdateNewsHandler _handler;

    public UpdateNewsTests()
    {
        _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
        _mockMapper = new Mock<IMapper>();
        _mockBlobService = new Mock<IBlobService>();
        _mockLogger = new Mock<ILoggerService>();

        _mockRepositoryWrapper.Setup(x => x.NewsRepository).Returns(new Mock<INewsRepository>().Object);
        _mockRepositoryWrapper.Setup(x => x.ImageRepository).Returns(new Mock<IImageRepository>().Object);

        _handler = new UpdateNewsHandler(
            _mockRepositoryWrapper.Object,
            _mockMapper.Object,
            _mockBlobService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task UpdateNews_WhenValidDTO_ShouldReturnSuccessWithDTO()
    {
        // Arrange
        var newsDTO = CreateNewsDTO();
        var newsEntity = CreateNewsEntity();

        _mockMapper.Setup(m => m.Map<DAL.Entities.News.News>(newsDTO)).Returns(newsEntity);
        _mockMapper.Setup(m => m.Map<NewsDTO>(newsEntity)).Returns(newsDTO);
        _mockBlobService.Setup(b => b.FindFileInStorageAsBase64(newsDTO.Image.BlobName)).Returns("base64string");
        _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        var command = new UpdateNewsCommand(newsDTO);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(newsDTO);
        result.Value.Image.Base64.Should().Be("base64string");

        _mockRepositoryWrapper.Verify(r => r.NewsRepository.Update(newsEntity), Times.Once);
        _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateNews_WhenDTOCannotBeMapped_ShouldReturnFailure()
    {
        // Arrange
        var newsDTO = CreateNewsDTO();

        _mockMapper
            .Setup(m => m.Map<DAL.Entities.News.News>(newsDTO))
            .Returns((DAL.Entities.News.News?)null);

        var command = new UpdateNewsCommand(newsDTO);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();

        _mockRepositoryWrapper.Verify(r => r.NewsRepository.Update(It.IsAny<DAL.Entities.News.News>()), Times.Never);
        _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateNews_WhenSaveChangesReturnsZero_ShouldReturnFailure()
    {
        // Arrange
        var newsDTO = CreateNewsDTO();
        var newsEntity = CreateNewsEntity(newsDTO.Id);

        _mockMapper.Setup(m => m.Map<DAL.Entities.News.News>(newsDTO))
            .Returns(newsEntity);

        _mockMapper.Setup(m => m.Map<NewsDTO>(newsEntity))
            .Returns(newsDTO);

        _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(0);

        var command = new UpdateNewsCommand(newsDTO);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();

        _mockRepositoryWrapper.Verify(r => r.NewsRepository.Update(newsEntity), Times.Once);
        _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    private static NewsDTO CreateNewsDTO()
    {
        return new NewsDTO
        {
            Id = 1,
            Title = "Test News",
            Text = "Test Content",
            URL = "test-url",
            ImageId = 1,
            Image = new ImageDTO { Id = 1, BlobName = "image.jpg" },
            CreationDate = DateTime.UtcNow
        };
    }

    private static DAL.Entities.News.News CreateNewsEntity(int id = 1)
    {
        return new DAL.Entities.News.News
        {
            Id = 1,
            Title = "Test News",
            Text = "Test Content",
            URL = "test-url",
            ImageId = 1,
            Image = new Image { Id = 1, BlobName = "image.jpg" },
            CreationDate = DateTime.UtcNow
        };
    }
}