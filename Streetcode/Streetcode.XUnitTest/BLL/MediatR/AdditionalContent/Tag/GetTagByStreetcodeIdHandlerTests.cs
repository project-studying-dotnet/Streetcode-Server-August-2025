using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.AdditionalContent.Tag;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.AdditionalContent.Tag.GetByStreetcodeId;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.DAL.Entities.AdditionalContent;
using Streetcode.DAL.Repositories.Interfaces.AdditionalContent;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.AdditionalContent.Tag;

public class GetTagByStreetcodeIdHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly Mock<IStreetcodeTagIndexRepository> _mockStreetcodeTagIndexRepository;
    private readonly GetTagByStreetcodeIdHandler _handler;

    public GetTagByStreetcodeIdHandlerTests()
    {
        _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILoggerService>();
        _mockStreetcodeTagIndexRepository = new Mock<IStreetcodeTagIndexRepository>();

        _mockRepositoryWrapper.Setup(x => x.StreetcodeTagIndexRepository)
            .Returns(_mockStreetcodeTagIndexRepository.Object);

        _handler = new GetTagByStreetcodeIdHandler(
            _mockRepositoryWrapper.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_WhenTagsExist_ShouldReturnSuccess()
    {
        // Arrange
        var request = new GetTagByStreetcodeIdQuery(1);
        var tagIndexedEntities = new List<StreetcodeTagIndex>
        {
            new StreetcodeTagIndex { StreetcodeId = 1, Index = 2, Tag = new DAL.Entities.AdditionalContent.Tag { Id = 2, Title = "Tag2" } },
            new StreetcodeTagIndex { StreetcodeId = 1, Index = 1, Tag = new DAL.Entities.AdditionalContent.Tag { Id = 1, Title = "Tag1" } }
        };
        var expectedDtos = new List<StreetcodeTagDTO>
        {
            new StreetcodeTagDTO { Id = 1, Title = "Tag1" },
            new StreetcodeTagDTO { Id = 2, Title = "Tag2" }
        };

        _mockStreetcodeTagIndexRepository.Setup(r => r.GetAllAsync(
                It.IsAny<Expression<Func<StreetcodeTagIndex, bool>>>(),
                It.IsAny<Func<IQueryable<StreetcodeTagIndex>, IIncludableQueryable<StreetcodeTagIndex, object>>>()))
            .ReturnsAsync(tagIndexedEntities);
        _mockMapper.Setup(m => m.Map<IEnumerable<StreetcodeTagDTO>>(It.IsAny<IEnumerable<StreetcodeTagIndex>>()))
            .Returns(expectedDtos);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedDtos);
        _mockStreetcodeTagIndexRepository.Verify(
            r => r.GetAllAsync(
            It.IsAny<Expression<Func<StreetcodeTagIndex, bool>>>(),
            It.IsAny<Func<IQueryable<StreetcodeTagIndex>, IIncludableQueryable<StreetcodeTagIndex, object>>>()), Times.Once);
        _mockMapper.Verify(m => m.Map<IEnumerable<StreetcodeTagDTO>>(It.IsAny<IEnumerable<StreetcodeTagIndex>>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenTagsDoNotExist_ShouldReturnFailure()
    {
        // Arrange
        var request = new GetTagByStreetcodeIdQuery(1);
        _mockStreetcodeTagIndexRepository.Setup(r => r.GetAllAsync(
                It.IsAny<Expression<Func<StreetcodeTagIndex, bool>>>(),
                It.IsAny<Func<IQueryable<StreetcodeTagIndex>, IIncludableQueryable<StreetcodeTagIndex, object>>>()))
            .ReturnsAsync((IEnumerable<StreetcodeTagIndex>)null);

        string expectedErrorMsg = Errors_Common.NotFoundByStreetcode.FormatWith("tag", request.StreetcodeId);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Errors.First().Message.Should().Be(expectedErrorMsg);
        _mockStreetcodeTagIndexRepository.Verify(
            r => r.GetAllAsync(
            It.IsAny<Expression<Func<StreetcodeTagIndex, bool>>>(),
            It.IsAny<Func<IQueryable<StreetcodeTagIndex>, IIncludableQueryable<StreetcodeTagIndex, object>>>()), Times.Once);
        _mockMapper.Verify(m => m.Map<IEnumerable<StreetcodeTagDTO>>(It.IsAny<IEnumerable<StreetcodeTagIndex>>()), Times.Never);
        _mockLogger.Verify(l => l.LogError(request, expectedErrorMsg), Times.Once);
    }
}