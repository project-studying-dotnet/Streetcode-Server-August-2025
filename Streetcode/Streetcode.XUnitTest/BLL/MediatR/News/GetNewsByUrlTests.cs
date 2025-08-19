using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Media.Images;
using Streetcode.BLL.DTO.News;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Newss.GetByUrl;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Repositories.Interfaces.Newss;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.News;

public class GetNewsByUrlTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
    private readonly Mock<INewsRepository> _mockNewsRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IBlobService> _mockBlobService;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly GetNewsByUrlHandler _handler;

    public GetNewsByUrlTests()
    {
        _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
        _mockNewsRepository = new Mock<INewsRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockBlobService = new Mock<IBlobService>();
        _mockLogger = new Mock<ILoggerService>();

        _mockRepositoryWrapper.Setup(x => x.NewsRepository).Returns(_mockNewsRepository.Object);

        _handler = new GetNewsByUrlHandler(
            _mockMapper.Object,
            _mockRepositoryWrapper.Object,
            _mockBlobService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task GetNewsByUrl_WhenNewsExists_ShouldReturnDTO()
    {
        // Arrange
        string newsUrl = "test-news-url";
        int newsId = 1;
        var newsEntity = CreateNewsEntity(newsId, newsUrl);
        var newsDTO = CreateNewsDTO(newsId, newsUrl);

        _mockNewsRepository
            .Setup(r => r.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<DAL.Entities.News.News, bool>>>(),
                It.IsAny<Func<IQueryable<DAL.Entities.News.News>, IIncludableQueryable<DAL.Entities.News.News, object>>>()))
            .ReturnsAsync(newsEntity);

        _mockMapper
            .Setup(m => m.Map<NewsDTO>(newsEntity))
            .Returns(newsDTO);

        var query = new GetNewsByUrlQuery(newsUrl);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(newsDTO);
        result.Value.URL.Should().Be(newsUrl);

        _mockMapper.Verify(m => m.Map<NewsDTO>(newsEntity), Times.Once);
    }

    [Fact]
    public async Task GetNewsByUrl_WhenNewsNotFound_ShouldReturnFailure()
    {
        // Arrange
        string newsUrl = "non-existing-url";

        _mockNewsRepository
            .Setup(r => r.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<DAL.Entities.News.News, bool>>>(),
                It.IsAny<Func<IQueryable<DAL.Entities.News.News>, IIncludableQueryable<DAL.Entities.News.News, object>>>()))
            .ReturnsAsync((DAL.Entities.News.News)null);

        _mockMapper
            .Setup(m => m.Map<NewsDTO>(null))
            .Returns((NewsDTO)null);

        var query = new GetNewsByUrlQuery(newsUrl);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();

        _mockMapper.Verify(m => m.Map<NewsDTO>(null), Times.Once);
    }

    [Fact]
    public async Task GetNewsByUrl_WhenNewsHasImage_ShouldSetBase64ForImage()
    {
        // Arrange
        string newsUrl = "news-with-image";
        int newsId = 1;
        var newsEntity = CreateNewsEntity(newsId, newsUrl);
        var newsDTO = CreateNewsDTOWithImage(newsId, "test-image.jpg", newsUrl);

        _mockNewsRepository
            .Setup(r => r.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<DAL.Entities.News.News, bool>>>(),
                It.IsAny<Func<IQueryable<DAL.Entities.News.News>, IIncludableQueryable<DAL.Entities.News.News, object>>>()))
            .ReturnsAsync(newsEntity);

        _mockMapper
            .Setup(m => m.Map<NewsDTO>(newsEntity))
            .Returns(newsDTO);

        _mockBlobService
            .Setup(b => b.FindFileInStorageAsBase64("test-image.jpg"))
            .Returns("base64-encoded-image-data");

        var query = new GetNewsByUrlQuery(newsUrl);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Image.Should().NotBeNull();
        result.Value.Image.Base64.Should().Be("base64-encoded-image-data");
        result.Value.Image.BlobName.Should().Be("test-image.jpg");
        result.Value.URL.Should().Be(newsUrl);

        _mockBlobService.Verify(b => b.FindFileInStorageAsBase64("test-image.jpg"), Times.Once);
        _mockMapper.Verify(m => m.Map<NewsDTO>(newsEntity), Times.Once);
    }

    private DAL.Entities.News.News CreateNewsEntity(int id, string url = null)
    {
        return new DAL.Entities.News.News
        {
            Id = id,
            Title = $"Test News {id}",
            Text = $"Test Text {id}",
            URL = url ?? $"test-news-{id}",
            CreationDate = DateTime.UtcNow,
            ImageId = null,
            Image = null
        };
    }

    private NewsDTO CreateNewsDTO(int id, string url = null)
    {
        return new NewsDTO
        {
            Id = id,
            Title = $"Test News {id}",
            Text = $"Test Text {id}",
            URL = url ?? $"test-news-{id}",
            CreationDate = DateTime.UtcNow,
            Image = null
        };
    }

    private NewsDTO CreateNewsDTOWithImage(int id, string blobName, string url = null)
    {
        return new NewsDTO
        {
            Id = id,
            Title = $"Test News {id}",
            Text = $"Test Text {id}",
            URL = url ?? $"test-news-{id}",
            CreationDate = DateTime.UtcNow,
            Image = new ImageDTO
            {
                BlobName = blobName,
                Base64 = null
            }
        };
    }
}