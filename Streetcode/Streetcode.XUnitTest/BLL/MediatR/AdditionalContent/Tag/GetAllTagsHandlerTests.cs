using AutoMapper;
using FluentAssertions;
using FluentResults;
using MediatR;
using Moq;
using Streetcode.BLL.DTO.AdditionalContent;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.AdditionalContent.Tag.GetAll;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.DAL.Entities.AdditionalContent;
using Streetcode.DAL.Repositories.Interfaces.AdditionalContent;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.AdditionalContent.Tag;

public class GetAllTagsHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly Mock<ITagRepository> _mockTagRepository;
    private readonly GetAllTagsHandler _handler;

    public GetAllTagsHandlerTests()
    {
        _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILoggerService>();
        _mockTagRepository = new Mock<ITagRepository>();

        _mockRepositoryWrapper.Setup(x => x.TagRepository).Returns(_mockTagRepository.Object);

        _handler = new GetAllTagsHandler(
            _mockRepositoryWrapper.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_WhenTagsExist_ShouldReturnSuccess()
    {
        // Arrange
        var testEntities = new List<DAL.Entities.AdditionalContent.Tag>
        {
            new DAL.Entities.AdditionalContent.Tag { Id = 1, Title = "Tag1" },
            new DAL.Entities.AdditionalContent.Tag { Id = 2, Title = "Tag2" }
        };
        var expectedDtos = new List<TagDTO>
        {
            new TagDTO { Id = 1, Title = "Tag1" },
            new TagDTO { Id = 2, Title = "Tag2" }
        };

        _mockTagRepository.Setup(r => r.GetAllAsync(null, null))
            .ReturnsAsync(testEntities);
        _mockMapper.Setup(m => m.Map<IEnumerable<TagDTO>>(testEntities))
            .Returns(expectedDtos);

        var query = new GetAllTagsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedDtos);
        _mockTagRepository.Verify(r => r.GetAllAsync(null, null), Times.Once);
        _mockMapper.Verify(m => m.Map<IEnumerable<TagDTO>>(It.IsAny<IEnumerable<DAL.Entities.AdditionalContent.Tag>>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenTagsDoNotExist_ShouldReturnFailure()
    {
        // Arrange
        _mockTagRepository.Setup(r => r.GetAllAsync(null, null))
            .ReturnsAsync((IEnumerable<DAL.Entities.AdditionalContent.Tag>)null);

        var query = new GetAllTagsQuery();
        string expectedErrorMsg = Errors_Common.NotFoundAny.FormatWith("tags");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Errors.First().Message.Should().Be(expectedErrorMsg);
        _mockTagRepository.Verify(r => r.GetAllAsync(null, null), Times.Once);
        _mockLogger.Verify(l => l.LogError(query, expectedErrorMsg), Times.Once);
        _mockMapper.Verify(m => m.Map<IEnumerable<TagDTO>>(It.IsAny<IEnumerable<DAL.Entities.AdditionalContent.Tag>>()), Times.Never);
    }
}