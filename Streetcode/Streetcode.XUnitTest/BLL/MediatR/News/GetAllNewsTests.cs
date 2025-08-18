using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Media.Images;
using Streetcode.BLL.DTO.News;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Newss.GetAll;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Repositories.Interfaces.Newss;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.News;

public class GetAllNewsTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
    private readonly Mock<INewsRepository> _mockNewsRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly Mock<IBlobService> _mockBlobService;
    private readonly GetAllNewsHandler _handler;

    public GetAllNewsTests()
    {
        _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
        _mockNewsRepository = new Mock<INewsRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILoggerService>();
        _mockBlobService = new Mock<IBlobService>();

        _mockRepositoryWrapper.Setup(r => r.NewsRepository)
            .Returns(_mockNewsRepository.Object);

        _handler = new GetAllNewsHandler(
            _mockRepositoryWrapper.Object,
            _mockMapper.Object,
            _mockBlobService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task GetAllNews_WhenNewsExist_ShouldReturnMappedDTOs()
    {
        // Arrange
        var newsList = new List<DAL.Entities.News.News>
        {
            CreateNewsWithoutImage(1),
            CreateNewsWithoutImage(2)
        };
        var newsDTOList = new List<NewsDTO>
        {
            CreateNewsDTO(1),
            CreateNewsDTO(2)
        };

        _mockNewsRepository
            .Setup(r => r.GetAllAsync(
                It.IsAny<Expression<Func<DAL.Entities.News.News, bool>>>(),
                It.IsAny<Func<IQueryable<DAL.Entities.News.News>, IIncludableQueryable<DAL.Entities.News.News, object>>>()))
            .ReturnsAsync(newsList);

        _mockMapper
            .Setup(m => m.Map<IEnumerable<NewsDTO>>(newsList))
            .Returns(newsDTOList);

        var query = new GetAllNewsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(newsDTOList);

        _mockNewsRepository.Verify(r => r.GetAllAsync(
            It.IsAny<Expression<Func<DAL.Entities.News.News, bool>>>(),
            It.IsAny<Func<IQueryable<DAL.Entities.News.News>, IIncludableQueryable<DAL.Entities.News.News, object>>>()), Times.Once);

        _mockMapper.Verify(m => m.Map<IEnumerable<NewsDTO>>(newsList), Times.Once);
    }

    [Fact]
    public async Task GetAllNews_WhenNewsHasImage_ShouldSetBase64FromBlobService()
    {
        // Arrange
        var newsList = new List<DAL.Entities.News.News>
        {
            CreateNewsWithoutImage(1)
        };
        var newsDTO = CreateNewsDTO(1);
        newsDTO.Image = new ImageDTO { BlobName = "test-blob.jpg" };
        var newsDTOList = new List<NewsDTO> { newsDTO };

        _mockNewsRepository
            .Setup(r => r.GetAllAsync(
                It.IsAny<Expression<Func<DAL.Entities.News.News, bool>>>(),
                It.IsAny<Func<IQueryable<DAL.Entities.News.News>, IIncludableQueryable<DAL.Entities.News.News, object>>>()))
            .ReturnsAsync(newsList);

        _mockMapper
            .Setup(m => m.Map<IEnumerable<NewsDTO>>(newsList))
            .Returns(newsDTOList);

        _mockBlobService
            .Setup(b => b.FindFileInStorageAsBase64("test-blob.jpg"))
            .Returns("base64-encoded-image-data");

        var query = new GetAllNewsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.First().Image.Base64.Should().Be("base64-encoded-image-data");

        _mockBlobService.Verify(b => b.FindFileInStorageAsBase64("test-blob.jpg"), Times.Once);
    }

    [Fact]
    public async Task GetAllNews_WhenRepositoryReturnsNull_ShouldReturnFailure()
    {
        // Arrange
        _mockNewsRepository
            .Setup(r => r.GetAllAsync(
                It.IsAny<Expression<Func<DAL.Entities.News.News, bool>>>(),
                It.IsAny<Func<IQueryable<DAL.Entities.News.News>, IIncludableQueryable<DAL.Entities.News.News, object>>>()))
            .ReturnsAsync((IEnumerable<DAL.Entities.News.News>)null);

        var query = new GetAllNewsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();

        _mockNewsRepository.Verify(r => r.GetAllAsync(
            It.IsAny<Expression<Func<DAL.Entities.News.News, bool>>>(),
            It.IsAny<Func<IQueryable<DAL.Entities.News.News>, IIncludableQueryable<DAL.Entities.News.News, object>>>()), Times.Once);
    }

    [Fact]
    public async Task GetAllNews_WhenRepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        var expectedException = new Exception("Database connection failed");

        _mockNewsRepository
            .Setup(r => r.GetAllAsync(
                It.IsAny<Expression<Func<DAL.Entities.News.News, bool>>>(),
                It.IsAny<Func<IQueryable<DAL.Entities.News.News>, IIncludableQueryable<DAL.Entities.News.News, object>>>()))
            .ThrowsAsync(expectedException);

        var query = new GetAllNewsQuery();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            _handler.Handle(query, CancellationToken.None));

        exception.Should().Be(expectedException);
    }

    // Test entities initializations
    private static DAL.Entities.News.News CreateNewsWithoutImage(int id = 1) => new()
    {
        Id = id,
        Title = $"Test News {id}",
        Text = "Test Content",
        URL = "test-url",
        ImageId = null,
        Image = null,
        CreationDate = DateTime.UtcNow
    };

    private static NewsDTO CreateNewsDTO(int id = 1, int? imageId = null) => new()
    {
        Id = id,
        Title = $"Test News {id}",
        Text = "Test Content",
        URL = "test-url",
        ImageId = imageId,
        Image = imageId.HasValue ? new ImageDTO { Id = imageId.Value } : null,
        CreationDate = DateTime.UtcNow
    };
}