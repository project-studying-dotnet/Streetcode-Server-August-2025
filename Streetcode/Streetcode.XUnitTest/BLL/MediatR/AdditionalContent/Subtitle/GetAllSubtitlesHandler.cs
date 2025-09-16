using AutoMapper;
using FluentAssertions;
using FluentResults;
using MediatR;
using Moq;
using Streetcode.BLL.DTO.AdditionalContent.Subtitles;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.AdditionalContent.Subtitle.GetAll;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.DAL.Entities.AdditionalContent;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Repositories.Interfaces.AdditionalContent;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.AdditionalContent.Subtitle;

public class GetAllSubtitlesHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly GetAllSubtitlesHandler _handler;
    private readonly Mock<ISubtitleRepository> _mockSubtitleRepository;

    public GetAllSubtitlesHandlerTests()
    {
        _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILoggerService>();
        _mockSubtitleRepository = new Mock<ISubtitleRepository>();

        _mockRepositoryWrapper.Setup(x => x.SubtitleRepository).Returns(_mockSubtitleRepository.Object);

        _handler = new GetAllSubtitlesHandler(
            _mockRepositoryWrapper.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_WhenSubtitlesExist_ShouldReturnSuccess()
    {
        // Arrange
        var testEntities = new List<DAL.Entities.AdditionalContent.Subtitle>
        {
            new DAL.Entities.AdditionalContent.Subtitle { Id = 1, SubtitleText = "Test Subtitle 1" },
            new DAL.Entities.AdditionalContent.Subtitle { Id = 2, SubtitleText = "Test Subtitle 2" }
        };
        var expectedDtos = new List<SubtitleDTO>
        {
            new SubtitleDTO { Id = 1, SubtitleText = "Test Subtitle 1" },
            new SubtitleDTO { Id = 2, SubtitleText = "Test Subtitle 2" }
        };

        _mockSubtitleRepository.Setup(r => r.GetAllAsync(null, null))
            .ReturnsAsync(testEntities);
        _mockMapper.Setup(m => m.Map<IEnumerable<SubtitleDTO>>(testEntities))
            .Returns(expectedDtos);

        var query = new GetAllSubtitlesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedDtos);
        _mockSubtitleRepository.Verify(r => r.GetAllAsync(null, null), Times.Once);
        _mockMapper.Verify(m => m.Map<IEnumerable<SubtitleDTO>>(It.IsAny<IEnumerable<DAL.Entities.AdditionalContent.Subtitle>>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenSubtitlesDoNotExist_ShouldReturnFailure()
    {
        // Arrange
        _mockSubtitleRepository.Setup(r => r.GetAllAsync(null, null))
            .ReturnsAsync((IEnumerable<DAL.Entities.AdditionalContent.Subtitle>)null);

        var query = new GetAllSubtitlesQuery();
        string expectedErrorMsg = Errors_Common.NotFoundAny.FormatWith("subtitles");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Errors.First().Message.Should().Be(expectedErrorMsg);
        _mockSubtitleRepository.Verify(r => r.GetAllAsync(null, null), Times.Once);
        _mockLogger.Verify(l => l.LogError(query, expectedErrorMsg), Times.Once);
        _mockMapper.Verify(m => m.Map<IEnumerable<SubtitleDTO>>(It.IsAny<IEnumerable<DAL.Entities.AdditionalContent.Subtitle>>()), Times.Never);
    }
}