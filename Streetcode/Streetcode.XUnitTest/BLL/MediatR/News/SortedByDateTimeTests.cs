using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Media.Images;
using Streetcode.BLL.DTO.News;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Newss.SortedByDateTime;
using Streetcode.DAL.Entities.Media.Images;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Repositories.Interfaces.Newss;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.News;

public class SortedByDateTimeTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
    private readonly Mock<INewsRepository> _mockNewsRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IBlobService> _mockBlobService;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly SortedByDateTimeHandler _handler;

    public SortedByDateTimeTests()
    {
        _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
        _mockNewsRepository = new Mock<INewsRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockBlobService = new Mock<IBlobService>();
        _mockLogger = new Mock<ILoggerService>();

        _mockRepositoryWrapper.Setup(x => x.NewsRepository).Returns(_mockNewsRepository.Object);

        _handler = new SortedByDateTimeHandler(
            _mockRepositoryWrapper.Object,
            _mockMapper.Object,
            _mockBlobService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task SortedByDateTime_WhenNewsExist_ShouldReturnSortedDTOs()
    {
        // Arrange
        var newsEntities = new List<DAL.Entities.News.News>
        {
            new() { Id = 1, Title = "News 1", CreationDate = DateTime.UtcNow.AddDays(-1), Image = new Image { BlobName = "blob1" } },
            new() { Id = 2, Title = "News 2", CreationDate = DateTime.UtcNow, Image = new Image { BlobName = "blob2" } },
            new() { Id = 3, Title = "News 3", CreationDate = DateTime.UtcNow.AddDays(-2) }
        };

        var newsDTOs = new List<NewsDTO>
        {
            new() { Id = 1, Title = "News 1", CreationDate = DateTime.UtcNow.AddDays(-1), Image = new ImageDTO { BlobName = "blob1" } },
            new() { Id = 2, Title = "News 2", CreationDate = DateTime.UtcNow, Image = new ImageDTO { BlobName = "blob2" } },
            new() { Id = 3, Title = "News 3", CreationDate = DateTime.UtcNow.AddDays(-2) }
        };

        _mockNewsRepository
            .Setup(r => r.GetAllAsync(
                It.IsAny<Expression<Func<DAL.Entities.News.News, bool>>>(),
                It.IsAny<Func<IQueryable<DAL.Entities.News.News>, IIncludableQueryable<DAL.Entities.News.News, object>>>()))
            .ReturnsAsync(newsEntities);

        _mockMapper
            .Setup(m => m.Map<List<NewsDTO>>(It.IsAny<List<DAL.Entities.News.News>>()))
            .Returns((List<DAL.Entities.News.News> src) =>
                src.Select(n => new NewsDTO
                {
                    Id = n.Id,
                    Title = n.Title,
                    CreationDate = n.CreationDate,
                    Image = n.Image is null ? null : new ImageDTO { BlobName = n.Image.BlobName }
                }).ToList());

        _mockBlobService
            .Setup(b => b.FindFileInStorageAsBase64(It.IsAny<string>()))
            .Returns("fake-base64");

        var query = new SortedByDateTimeQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
        result.Value[0].Id.Should().Be(2); // newest
        result.Value[1].Id.Should().Be(1);
        result.Value[2].Id.Should().Be(3);

        // Ensure Base64 populated
        result.Value[0].Image!.Base64.Should().Be("fake-base64");
        result.Value[1].Image!.Base64.Should().Be("fake-base64");
        result.Value[2].Image.Should().BeNull(); // had no image
    }

    [Fact]
    public async Task SortedByDateTime_WhenNoNewsExist_ShouldReturnFailure()
    {
        // Arrange
        _mockNewsRepository
            .Setup(r => r.GetAllAsync(
                It.IsAny<Expression<Func<DAL.Entities.News.News, bool>>>(),
                It.IsAny<Func<IQueryable<DAL.Entities.News.News>, IIncludableQueryable<DAL.Entities.News.News, object>>>()))
            .ReturnsAsync(Enumerable.Empty<DAL.Entities.News.News>());

        var query = new SortedByDateTimeQuery();
        string errorMsg = Errors_Common.NotFoundAny.FormatWith("news");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Errors[0].Message.Should().Be(errorMsg);
    }
}