using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.News;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Newss.GetNewsAndLinksByUrl;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Repositories.Interfaces.Newss;
using System.Linq.Expressions;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.News;

public class GetNewsAndLinksByUrlTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
    private readonly Mock<INewsRepository> _mockNewsRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IBlobService> _mockBlobService;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly GetNewsAndLinksByUrlHandler _handler;


    public GetNewsAndLinksByUrlTests()
    {
        _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
        _mockNewsRepository = new Mock<INewsRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockBlobService = new Mock<IBlobService>();
        _mockLogger = new Mock<ILoggerService>();

        _mockRepositoryWrapper.Setup(x => x.NewsRepository).Returns(_mockNewsRepository.Object);

        _handler = new GetNewsAndLinksByUrlHandler(
            _mockMapper.Object,
            _mockRepositoryWrapper.Object,
            _mockBlobService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task GetNewsAndLinksByUrl_WhenNewsExists_ShouldReturnDTOWithUrls()
    {
        // Arrange
        var testUrl = "test-news-url";
        var query = new GetNewsAndLinksByUrlQuery(testUrl);

        // Create test entities
        var newsEntity = CreateNewsEntity(1, testUrl);
        var allNewsEntities = new List<DAL.Entities.News.News>
        {
            CreateNewsEntity(1, "first-url"),
            CreateNewsEntity(2, testUrl), // target news
            CreateNewsEntity(3, "third-url"),
            CreateNewsEntity(4, "fourth-url")
        };


        var newsDTO = CreateNewsDTO(2, testUrl);

        _mockNewsRepository
            .Setup(x => x.GetFirstOrDefaultAsync(
                It.Is<Expression<Func<DAL.Entities.News.News, bool>>>(expr => true),
                It.IsAny<Func<IQueryable<DAL.Entities.News.News>, IIncludableQueryable<DAL.Entities.News.News, object>>>()))
            .ReturnsAsync(newsEntity);

        _mockNewsRepository
            .Setup(r => r.GetAllAsync(
                It.IsAny<Expression<Func<DAL.Entities.News.News, bool>>>(),
                It.IsAny<Func<IQueryable<DAL.Entities.News.News>, IIncludableQueryable<DAL.Entities.News.News, object>>>()))
            .ReturnsAsync(allNewsEntities);

        _mockMapper
            .Setup(x => x.Map<NewsDTO>(It.IsAny<DAL.Entities.News.News>()))
            .Returns(newsDTO);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        var returnedData = result.Value;

        returnedData.News.Should().NotBeNull();
        returnedData.News.Id.Should().Be(2);
        returnedData.News.URL.Should().Be(testUrl);

        returnedData.PrevNewsUrl.Should().Be("first-url");
        returnedData.NextNewsUrl.Should().Be("third-url");

        // Verify random news
        returnedData.RandomNews.Should().NotBeNull();
        returnedData.RandomNews.RandomNewsUrl.Should().Be("fourth-url");
        returnedData.RandomNews.Title.Should().Be("Test News 4");

        // Verify repository calls
        _mockNewsRepository.Verify(
            x => x.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<DAL.Entities.News.News, bool>>>(),
                It.IsAny<Func<IQueryable<DAL.Entities.News.News>, IIncludableQueryable<DAL.Entities.News.News, object>>>()),
            Times.Once);


        // Verify mapper call
        _mockMapper.Verify(x => x.Map<NewsDTO>(It.IsAny<DAL.Entities.News.News>()), Times.Once);
    }


    [Fact]
    public async Task GetNewsAndLinksByUrl_WhenNewsNotFound_ShouldReturnFailure()
    {
        // Arrange
        var testUrl = "test-news-url";
        var query = new GetNewsAndLinksByUrlQuery(testUrl);

        _mockNewsRepository
            .Setup(x => x.GetFirstOrDefaultAsync(
                It.Is<Expression<Func<DAL.Entities.News.News, bool>>>(expr => true),
                It.IsAny<Func<IQueryable<DAL.Entities.News.News>, IIncludableQueryable<DAL.Entities.News.News, object>>>()))
            .ReturnsAsync((DAL.Entities.News.News?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }


    [Fact]
    public async Task GetNewsAndLinksByUrl_WhenTotalNewsLessOrEqualThree_ShouldSetRandomNewsAsCurrent()
    {
        // Arrange
        var testUrl = "test-news-url";
        var query = new GetNewsAndLinksByUrlQuery(testUrl);

        var newsEntity = CreateNewsEntity(2, testUrl);
        var allNewsEntities = new List<DAL.Entities.News.News>
        {
            CreateNewsEntity(1, "first-url"),
            newsEntity,
            CreateNewsEntity(3, "third-url")
        };

        var newsDTO = CreateNewsDTO(2, testUrl);

        _mockNewsRepository
            .Setup(x => x.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<DAL.Entities.News.News, bool>>>(),
                It.IsAny<Func<IQueryable<DAL.Entities.News.News>, IIncludableQueryable<DAL.Entities.News.News, object>>>()))
            .ReturnsAsync(newsEntity);

        _mockNewsRepository
            .Setup(r => r.GetAllAsync(
                It.IsAny<Expression<Func<DAL.Entities.News.News, bool>>>(),
                It.IsAny<Func<IQueryable<DAL.Entities.News.News>, IIncludableQueryable<DAL.Entities.News.News, object>>>()))
            .ReturnsAsync(allNewsEntities);

        _mockMapper
            .Setup(x => x.Map<NewsDTO>(It.IsAny<DAL.Entities.News.News>()))
            .Returns(newsDTO);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        var returnedData = result.Value;

        returnedData.RandomNews.Should().NotBeNull();
        returnedData.RandomNews.Title.Should().Be(newsDTO.Title);
        returnedData.RandomNews.RandomNewsUrl.Should().Be(newsDTO.URL);
    }

    private DAL.Entities.News.News CreateNewsEntity(int id, string url = null)
    {
        return new DAL.Entities.News.News
        {
            Id = id,
            Title = $"Test News {id}",
            Text = "Test Content",
            URL = url,
            ImageId = 10,
            Image = null,
            CreationDate = DateTime.UtcNow
        };
    }

    private NewsDTO CreateNewsDTO(int id, string url = null)
    {
        return new NewsDTO
        {
            Id = id,
            Title = $"Test News {id}",
            Text = "Test Content",
            URL = url,
            ImageId = 10,
            Image = null
        };
    }

    private NewsDTOWithURLs CreateNewsDTOWithURLs()
    {
        return new NewsDTOWithURLs
        {
            News = new NewsDTO
            {
                Id = 1,
                Title = "Test News 1",
                Text = "Test Content",
                URL = "test-news-url"
            },
            PrevNewsUrl = null,
            NextNewsUrl = "second-url",
            RandomNews = new RandomNewsDTO
            {
                Title = "Third News",
                RandomNewsUrl = "third-url"
            }
        };
    }


}