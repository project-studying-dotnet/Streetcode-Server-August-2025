using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using FluentResults;
using MediatR;
using Moq;
using Streetcode.BLL.DTO.AdditionalContent;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.AdditionalContent.Tag.GetById;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.DAL.Entities.AdditionalContent;
using Streetcode.DAL.Repositories.Interfaces.AdditionalContent;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.AdditionalContent.Tag;

public class GetTagByIdHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly Mock<ITagRepository> _mockTagRepository;
    private readonly GetTagByIdHandler _handler;

    public GetTagByIdHandlerTests()
    {
        _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILoggerService>();
        _mockTagRepository = new Mock<ITagRepository>();

        _mockRepositoryWrapper.Setup(x => x.TagRepository).Returns(_mockTagRepository.Object);

        _handler = new GetTagByIdHandler(
            _mockRepositoryWrapper.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_WhenTagExists_ShouldReturnSuccess()
    {
        // Arrange
        var request = new GetTagByIdQuery(1);
        var tagEntity = new DAL.Entities.AdditionalContent.Tag { Id = 1, Title = "ExistingTag" };
        var expectedDto = new TagDTO { Id = 1, Title = "ExistingTag" };

        _mockTagRepository.Setup(r => r.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<DAL.Entities.AdditionalContent.Tag, bool>>>(), null))
            .ReturnsAsync(tagEntity);
        _mockMapper.Setup(m => m.Map<TagDTO>(tagEntity)).Returns(expectedDto);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedDto);
        _mockTagRepository.Verify(r => r.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<DAL.Entities.AdditionalContent.Tag, bool>>>(), null), Times.Once);
        _mockMapper.Verify(m => m.Map<TagDTO>(It.IsAny<DAL.Entities.AdditionalContent.Tag>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenTagDoesNotExist_ShouldReturnFailure()
    {
        // Arrange
        var request = new GetTagByIdQuery(1);

        _mockTagRepository.Setup(r => r.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<DAL.Entities.AdditionalContent.Tag, bool>>>(), null))
            .ReturnsAsync((DAL.Entities.AdditionalContent.Tag)null);

        string expectedErrorMsg = Errors_Common.NotFoundById.FormatWith("Tag", request.Id);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Errors.First().Message.Should().Be(expectedErrorMsg);
        _mockTagRepository.Verify(r => r.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<DAL.Entities.AdditionalContent.Tag, bool>>>(), null), Times.Once);
        _mockLogger.Verify(l => l.LogError(request, expectedErrorMsg), Times.Once);
        _mockMapper.Verify(m => m.Map<TagDTO>(It.IsAny<DAL.Entities.AdditionalContent.Tag>()), Times.Never);
    }
}