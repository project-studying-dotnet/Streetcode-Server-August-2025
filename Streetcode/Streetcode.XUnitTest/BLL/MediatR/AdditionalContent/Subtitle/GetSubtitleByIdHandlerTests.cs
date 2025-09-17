using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using FluentResults;
using Moq;
using Streetcode.BLL.DTO.AdditionalContent.Subtitles;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.AdditionalContent.GetById;
using Streetcode.BLL.MediatR.AdditionalContent.Subtitle.GetById;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.DAL.Entities.AdditionalContent;
using Streetcode.DAL.Repositories.Interfaces.AdditionalContent;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.AdditionalContent.Subtitle;

public class GetSubtitleByIdHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly Mock<ISubtitleRepository> _mockSubtitleRepository;
    private readonly GetSubtitleByIdHandler _handler;

    public GetSubtitleByIdHandlerTests()
    {
        _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILoggerService>();
        _mockSubtitleRepository = new Mock<ISubtitleRepository>();

        _mockRepositoryWrapper.Setup(x => x.SubtitleRepository)
            .Returns(_mockSubtitleRepository.Object);

        _handler = new GetSubtitleByIdHandler(
            _mockRepositoryWrapper.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_WhenSubtitleExists_ShouldReturnSuccess()
    {
        // Arrange
        var request = new GetSubtitleByIdQuery(1);
        var subtitleEntity = new DAL.Entities.AdditionalContent.Subtitle { Id = 1, SubtitleText = "Test Subtitle" };
        var expectedDto = new SubtitleDTO { Id = 1, SubtitleText = "Test Subtitle" };

        _mockSubtitleRepository.Setup(r => r.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<DAL.Entities.AdditionalContent.Subtitle, bool>>>(), null))
            .ReturnsAsync(subtitleEntity);
        _mockMapper.Setup(m => m.Map<SubtitleDTO>(subtitleEntity))
            .Returns(expectedDto);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedDto);
        _mockSubtitleRepository.Verify(r => r.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<DAL.Entities.AdditionalContent.Subtitle, bool>>>(), null), Times.Once);
        _mockMapper.Verify(m => m.Map<SubtitleDTO>(It.IsAny<DAL.Entities.AdditionalContent.Subtitle>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenSubtitleDoesNotExist_ShouldReturnFailure()
    {
        // Arrange
        var request = new GetSubtitleByIdQuery(1);

        _mockSubtitleRepository.Setup(r => r.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<DAL.Entities.AdditionalContent.Subtitle, bool>>>(), null))
            .ReturnsAsync((Streetcode.DAL.Entities.AdditionalContent.Subtitle)null);

        string expectedErrorMsg = Errors_Common.NotFoundById.FormatWith("subtitle", request.Id);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Errors.First().Message.Should().Be(expectedErrorMsg);
        _mockSubtitleRepository.Verify(r => r.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<DAL.Entities.AdditionalContent.Subtitle, bool>>>(), null), Times.Once);
        _mockLogger.Verify(l => l.LogError(request, expectedErrorMsg), Times.Once);
        _mockMapper.Verify(m => m.Map<SubtitleDTO>(It.IsAny<DAL.Entities.AdditionalContent.Subtitle>()), Times.Never);
    }
}