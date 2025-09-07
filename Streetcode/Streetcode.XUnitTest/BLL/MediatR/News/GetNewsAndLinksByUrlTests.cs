using System.Linq.Expressions;
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
    public async Task GetNewsAndLinksByUrl_WhenNewsNotFound_ShouldReturnFailure()
    {
        // Arrange
        var testUrl = "test-news-url";
        var query = new GetNewsAndLinksByUrlQuery(testUrl);

        _mockNewsRepository
            .Setup(x => x.GetFirstOrDefaultAsync(
                It.Is<Expression<Func<DAL.Entities.News.News, bool>>>(expr => true),
                It.IsAny<Func<IQueryable<DAL.Entities.News.News>,
                    IIncludableQueryable<DAL.Entities.News.News, object>>>()))
            .ReturnsAsync((DAL.Entities.News.News?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
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
}