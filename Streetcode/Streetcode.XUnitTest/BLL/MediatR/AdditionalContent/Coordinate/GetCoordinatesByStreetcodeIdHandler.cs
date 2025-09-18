using AutoMapper;
using FluentAssertions;
using FluentResults;
using Moq;
using Streetcode.BLL.DTO.AdditionalContent.Coordinates.Types;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.AdditionalContent.Coordinate.GetByStreetcodeId;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.DAL.Entities.AdditionalContent.Coordinates.Types;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.AdditionalContent.Coordinate;

public class GetCoordinatesByStreetcodeIdHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly GetCoordinatesByStreetcodeIdHandler _handler;

    public GetCoordinatesByStreetcodeIdHandlerTests()
    {
        _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILoggerService>();

        _mockRepositoryWrapper.Setup(x => x.StreetcodeRepository)
            .Returns(Mock.Of<DAL.Repositories.Interfaces.Streetcode.IStreetcodeRepository>());
        _mockRepositoryWrapper.Setup(x => x.StreetcodeCoordinateRepository)
            .Returns(Mock.Of<DAL.Repositories.Interfaces.AdditionalContent.IStreetcodeCoordinateRepository>());

        _handler = new GetCoordinatesByStreetcodeIdHandler(
            _mockRepositoryWrapper.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_WhenStreetcodeAndCoordinatesExist_ShouldReturnSuccess()
    {
        // Arrange
        var request = new GetCoordinatesByStreetcodeIdQuery(1);
        var streetcodeEntity = new StreetcodeContent { Id = 1 };
        var coordinateEntities = new List<StreetcodeCoordinate>
        {
            new StreetcodeCoordinate { Id = 1, StreetcodeId = 1 },
            new StreetcodeCoordinate { Id = 2, StreetcodeId = 1 }
        };
        var expectedDtos = new List<StreetcodeCoordinateDTO>
        {
            new StreetcodeCoordinateDTO { Id = 1, StreetcodeId = 1 },
            new StreetcodeCoordinateDTO { Id = 2, StreetcodeId = 1 }
        };

        _mockRepositoryWrapper.Setup(r => r.StreetcodeRepository.GetFirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<System.Func<StreetcodeContent, bool>>>(),
                null))
            .ReturnsAsync(streetcodeEntity);

        _mockRepositoryWrapper.Setup(r => r.StreetcodeCoordinateRepository.GetAllAsync(
                It.IsAny<System.Linq.Expressions.Expression<System.Func<StreetcodeCoordinate, bool>>>(),
                null))
            .ReturnsAsync(coordinateEntities);

        _mockMapper.Setup(m => m.Map<IEnumerable<StreetcodeCoordinateDTO>>(coordinateEntities))
            .Returns(expectedDtos);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedDtos);
        _mockRepositoryWrapper.Verify(
            r => r.StreetcodeRepository.GetFirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<StreetcodeContent, bool>>>(), null), Times.Once);
        _mockRepositoryWrapper.Verify(
            r => r.StreetcodeCoordinateRepository.GetAllAsync(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<StreetcodeCoordinate, bool>>>(), null), Times.Once);
        _mockMapper.Verify(m => m.Map<IEnumerable<StreetcodeCoordinateDTO>>(It.IsAny<IEnumerable<StreetcodeCoordinate>>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenStreetcodeDoesNotExist_ShouldReturnFailure()
    {
        // Arrange
        var request = new GetCoordinatesByStreetcodeIdQuery(1);

        _mockRepositoryWrapper.Setup(r => r.StreetcodeRepository.GetFirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<System.Func<StreetcodeContent, bool>>>(),
                null))
            .ReturnsAsync((StreetcodeContent)null);

        var expectedError = Errors_AdditionalContent.Coordinate_NotFound_StreetcodeDoesNotExist.FormatWith(request.StreetcodeId);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Errors.First().Message.Should().Contain(expectedError);
        _mockRepositoryWrapper.Verify(
            r => r.StreetcodeRepository.GetFirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<StreetcodeContent, bool>>>(), null), Times.Once);
        _mockRepositoryWrapper.Verify(
            r => r.StreetcodeCoordinateRepository.GetAllAsync(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<StreetcodeCoordinate, bool>>>(), null), Times.Never);
        _mockLogger.Verify(l => l.LogError(It.IsAny<GetCoordinatesByStreetcodeIdQuery>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCoordinatesAreNotFoundForStreetcode_ShouldReturnFailure()
    {
        // Arrange
        var request = new GetCoordinatesByStreetcodeIdQuery(1);
        var streetcodeEntity = new StreetcodeContent { Id = 1 };

        _mockRepositoryWrapper.Setup(r => r.StreetcodeRepository.GetFirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<System.Func<StreetcodeContent, bool>>>(),
                null))
            .ReturnsAsync(streetcodeEntity);

        _mockRepositoryWrapper.Setup(r => r.StreetcodeCoordinateRepository.GetAllAsync(
                It.IsAny<System.Linq.Expressions.Expression<System.Func<StreetcodeCoordinate, bool>>>(),
                null))
            .ReturnsAsync((List<StreetcodeCoordinate>)null);

        var expectedError = Errors_Common.NotFoundByStreetcode.FormatWith("coordinates", request.StreetcodeId);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Errors.First().Message.Should().Contain(expectedError);
        _mockRepositoryWrapper.Verify(
            r => r.StreetcodeRepository.GetFirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<StreetcodeContent, bool>>>(), null), Times.Once);
        _mockRepositoryWrapper.Verify(
            r => r.StreetcodeCoordinateRepository.GetAllAsync(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<StreetcodeCoordinate, bool>>>(), null), Times.Once);
        _mockLogger.Verify(l => l.LogError(It.IsAny<GetCoordinatesByStreetcodeIdQuery>(), It.IsAny<string>()), Times.Once);
    }
}