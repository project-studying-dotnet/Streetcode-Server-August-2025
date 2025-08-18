using System.Linq.Expressions;
using FluentAssertions;
using Moq;
using Repositories.Interfaces;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Newss.Delete;
using Streetcode.DAL.Entities.Media.Images;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Repositories.Interfaces.Newss;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.News;

public class DeleteNewsTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
    private readonly Mock<INewsRepository> _mockNewsRepository;
    private readonly Mock<IImageRepository> _mockImageRepository;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly DeleteNewsHandler _handler;

    public DeleteNewsTests()
    {
        _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
        _mockNewsRepository = new Mock<INewsRepository>();
        _mockImageRepository = new Mock<IImageRepository>();
        _mockLogger = new Mock<ILoggerService>();

        _mockRepositoryWrapper.Setup(repo => repo.NewsRepository)
            .Returns(_mockNewsRepository.Object);
        _mockRepositoryWrapper.Setup(repo => repo.ImageRepository)
            .Returns(_mockImageRepository.Object);

        _handler = new DeleteNewsHandler(
            _mockRepositoryWrapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task DeleteNews_WhenValidIdAndNewsExists_ShouldReturnSuccessAndDeleteNews()
    {
        // Arrange
        var command = CreateDeleteCommand(1);
        var news = CreateNewsWithoutImage(1);

        _mockNewsRepository.Setup(r => r.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<DAL.Entities.News.News, bool>>>(), null))
            .ReturnsAsync(news);

        _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        _mockNewsRepository.Verify(r => r.Delete(news), Times.Once);
        _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Once);
        _mockImageRepository.Verify(r => r.Delete(It.IsAny<Image>()), Times.Never);

    }

    [Fact]
    public async Task DeleteNews_WhenNewsHasImage_ShouldDeleteBothNewsAndImage()
    {
        // Arrange
        var command = CreateDeleteCommand(1);
        var newsWithImage = CreateNewsWithImage(1, 10);

        _mockNewsRepository
            .Setup(r => r.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<DAL.Entities.News.News, bool>>>(),
                null))
            .ReturnsAsync(newsWithImage);

        _mockRepositoryWrapper
            .Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        _mockImageRepository.Verify(r => r.Delete(newsWithImage.Image), Times.Once);
        _mockNewsRepository.Verify(r => r.Delete(newsWithImage), Times.Once);
        _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteNews_WhenNewsNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = CreateDeleteCommand(1);

        _mockNewsRepository
            .Setup(r => r.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<DAL.Entities.News.News, bool>>>(), null))
            .ReturnsAsync((DAL.Entities.News.News?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();

        _mockNewsRepository.Verify(r => r.Delete(It.IsAny<DAL.Entities.News.News>()), Times.Never);
        _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteNews_WhenSaveChangesReturnsZero_ShouldReturnFailure()
    {
        // Arrange
        var command = CreateDeleteCommand(1);
        var news = CreateNewsWithoutImage(1);

        _mockNewsRepository
            .Setup(r => r.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<DAL.Entities.News.News, bool>>>(), null))
            .ReturnsAsync(news);

        _mockRepositoryWrapper
            .Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(0);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();

        _mockNewsRepository.Verify(r => r.Delete(news), Times.Once);
        _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteNews_WhenRepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        var command = CreateDeleteCommand(1);
        var news = CreateNewsWithoutImage(1);

        _mockNewsRepository
            .Setup(r => r.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<DAL.Entities.News.News, bool>>>(), null))
            .ReturnsAsync(news);

        _mockNewsRepository
            .Setup(r => r.Delete(news))
            .Throws<InvalidOperationException>();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        _mockNewsRepository.Verify(r => r.Delete(news), Times.Once);
        _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    // Test entities initializations
    private static DeleteNewsCommand CreateDeleteCommand(int id = 1) => new(id);

    private static DAL.Entities.News.News CreateNewsWithoutImage(int id = 1) => new()
    {
        Id = id,
        Title = "Test News",
        Text = "Test Content",
        URL = "test-url",
        ImageId = null,
        Image = null,
        CreationDate = DateTime.UtcNow
    };

    private static DAL.Entities.News.News CreateNewsWithImage(int id = 1, int imageId = 1) => new()
    {
        Id = id,
        Title = "Test News",
        Text = "Test Content",
        URL = "test-url",
        ImageId = imageId,
        Image = new Image { Id = imageId },
        CreationDate = DateTime.UtcNow
    };
}