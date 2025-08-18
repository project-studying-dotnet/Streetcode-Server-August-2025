using AutoMapper;
using FluentAssertions;
using Moq;
using Streetcode.BLL.DTO.News;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Newss.Create;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Repositories.Interfaces.Newss;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.News;

public class CreateNewsTests
{
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
    private readonly Mock<INewsRepository> _mockNewsRepository;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly CreateNewsHandler _handler;

    public CreateNewsTests()
    {
        _mockMapper = new Mock<IMapper>();
        _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
        _mockNewsRepository = new Mock<INewsRepository>();
        _mockLogger = new Mock<ILoggerService>();

        _mockRepositoryWrapper.Setup(repo => repo.NewsRepository)
            .Returns(_mockNewsRepository.Object);

        _handler = new CreateNewsHandler(
            _mockMapper.Object,
            _mockRepositoryWrapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task CreateNews_WhenValidNewsData_ShouldReturnSuccessWithNewsDTO()
    {
        // Arrange
        var newsDTO = CreateValidNewsDTO();
        var newsEntity = CreateValidNewsEntity();
        var createdEntity = CreateValidNewsEntity();
        var expectedResultDTO = CreateValidNewsDTO(id: 1);

        var command = new CreateNewsCommand(newsDTO);

        _mockMapper.Setup(m => m.Map<DAL.Entities.News.News>(newsDTO))
            .Returns(newsEntity);
        _mockNewsRepository.Setup(r => r.Create(newsEntity))
            .Returns(createdEntity);
        _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);
        _mockMapper.Setup(m => m.Map<NewsDTO>(createdEntity))
            .Returns(expectedResultDTO);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedResultDTO);

        _mockNewsRepository.Verify(r => r.Create(newsEntity), Times.Once);
        _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.AtLeast(1));
    }

    [Fact]
    public async Task CreateNews_WhenMapperReturnsNull_ShouldReturnFailureWithErrorMessage()
    {
        // Arrange
        var newsDTO = CreateValidNewsDTO();
        var command = new CreateNewsCommand(newsDTO);

        _mockMapper.Setup(m => m.Map<DAL.Entities.News.News>(newsDTO))
            .Returns((DAL.Entities.News.News)null!);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();

        _mockNewsRepository.Verify(r => r.Create(It.IsAny<DAL.Entities.News.News>()), Times.Never);
    }

    [Fact]
    public async Task CreateNews_WhenImageIdIsZero_ShouldSetImageIdToNull()
    {
        // Arrange
        var newsDTO = CreateValidNewsDTO(imageId: 0);
        var mappedNewsEntity = CreateValidNewsEntity(imageId: 0);
        var createdEntity = CreateValidNewsEntity(imageId: null);
        var expectedResultDTO = CreateValidNewsDTO(id: 1, imageId: null);

        var command = new CreateNewsCommand(newsDTO);

        // Variable to capture the entity passed to Create method
        DAL.Entities.News.News? capturedEntity = null;

        _mockMapper.Setup(m => m.Map<DAL.Entities.News.News>(newsDTO))
            .Returns(mappedNewsEntity);

        _mockNewsRepository.Setup(r => r.Create(It.IsAny<DAL.Entities.News.News>()))
            .Callback<DAL.Entities.News.News>(entity => capturedEntity = entity)
            .Returns(createdEntity);

        _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        _mockMapper.Setup(m => m.Map<NewsDTO>(createdEntity))
            .Returns(expectedResultDTO);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedResultDTO);

        capturedEntity.Should().NotBeNull();
        capturedEntity!.ImageId.Should().BeNull("handler should set ImageId to null when it's 0");

        _mockNewsRepository.Verify(r => r.Create(It.IsAny<DAL.Entities.News.News>()), Times.Once);
        _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.AtLeast(1));
    }

    [Fact]
    public async Task CreateNews_WhenSaveChangesReturnsZero_ShouldReturnFailureWithErrorMessage()
    {
        // Arrange
        var newsDTO = CreateValidNewsDTO();
        var newsEntity = CreateValidNewsEntity();
        var command = new CreateNewsCommand(newsDTO);

        _mockMapper.Setup(m => m.Map<DAL.Entities.News.News>(newsDTO))
            .Returns(newsEntity);
        _mockNewsRepository.Setup(r => r.Create(newsEntity))
            .Returns(newsEntity);
        _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(0);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();

        _mockNewsRepository.Verify(r => r.Create(newsEntity), Times.Once);
        _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateNews_WhenRepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        var newsDTO = CreateValidNewsDTO();
        var newsEntity = CreateValidNewsEntity();
        var command = new CreateNewsCommand(newsDTO);

        _mockMapper.Setup(m => m.Map<DAL.Entities.News.News>(newsDTO))
            .Returns(newsEntity);
        _mockNewsRepository.Setup(r => r.Create(newsEntity))
            .Throws<InvalidOperationException>();

        // Act and Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        _mockNewsRepository.Verify(r => r.Create(newsEntity), Times.Once);
        _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    // Test entities initializations
    private static NewsDTO CreateValidNewsDTO(int id = 0, int? imageId = 1)
    {
        return new NewsDTO
        {
            Id = id,
            Title = "Test News Title",
            Text = "Test news content",
            URL = "test-news-url",
            ImageId = imageId,
            CreationDate = DateTime.UtcNow
        };
    }

    private static DAL.Entities.News.News CreateValidNewsEntity(int id = 1, int? imageId = 1)
    {
        return new DAL.Entities.News.News
        {
            Id = id,
            Title = "Test News Title",
            Text = "Test news content",
            URL = "test-news-url",
            ImageId = imageId,
            CreationDate = DateTime.UtcNow
        };
    }
}