using AutoMapper;
using FluentAssertions;
using FluentResults;
using MediatR;
using Moq;
using Streetcode.BLL.MediatR.AdditionalContent.Coordinate.Delete;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.DAL.Entities.AdditionalContent.Coordinates.Types;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Repositories.Interfaces.AdditionalContent;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.AdditionalContent.Coordinate;

public class DeleteCoordinateHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
    private readonly Mock<IStreetcodeCoordinateRepository> _mockStreetcodeCoordinateRepository;
    private readonly DeleteCoordinateHandler _handler;

    public DeleteCoordinateHandlerTests()
    {
        _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
        _mockStreetcodeCoordinateRepository = new Mock<IStreetcodeCoordinateRepository>();

        _mockRepositoryWrapper.Setup(x => x.StreetcodeCoordinateRepository)
            .Returns(_mockStreetcodeCoordinateRepository.Object);

        _handler = new DeleteCoordinateHandler(
            _mockRepositoryWrapper.Object);
    }

    [Fact]
    public async Task Handle_WhenCoordinateExists_ShouldReturnSuccess()
    {
        // Arrange
        var request = new DeleteCoordinateCommand(1);
        var coordinateEntity = new StreetcodeCoordinate { Id = 1 };

        _mockStreetcodeCoordinateRepository.Setup(r => r.GetFirstOrDefaultAsync(c => c.Id == It.IsAny<int>(), null))
            .ReturnsAsync(coordinateEntity);

        _mockStreetcodeCoordinateRepository.Setup(r => r.Delete(coordinateEntity));
        _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(Unit.Value);
        _mockStreetcodeCoordinateRepository.Verify(r => r.GetFirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<StreetcodeCoordinate, bool>>>(), null), Times.Once);
        _mockStreetcodeCoordinateRepository.Verify(r => r.Delete(It.IsAny<StreetcodeCoordinate>()), Times.Once);
        _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCoordinateDoesNotExist_ShouldReturnFailure()
    {
        // Arrange
        var request = new DeleteCoordinateCommand(1);

        _mockStreetcodeCoordinateRepository.Setup(r => r.GetFirstOrDefaultAsync(c => c.Id == It.IsAny<int>(), null))
            .ReturnsAsync((StreetcodeCoordinate)null);

        var expectedError = Errors_AdditionalContent.Coordinate_NotFoundByCategory.FormatWith(request.Id);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Errors.First().Message.Should().Contain(expectedError);
        _mockStreetcodeCoordinateRepository.Verify(r => r.GetFirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<StreetcodeCoordinate, bool>>>(), null), Times.Once);
        _mockStreetcodeCoordinateRepository.Verify(r => r.Delete(It.IsAny<StreetcodeCoordinate>()), Times.Never);
        _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenSaveChangesFails_ShouldReturnFailure()
    {
        // Arrange
        var request = new DeleteCoordinateCommand(1);
        var coordinateEntity = new StreetcodeCoordinate { Id = 1 };

        _mockStreetcodeCoordinateRepository.Setup(r => r.GetFirstOrDefaultAsync(c => c.Id == It.IsAny<int>(), null))
            .ReturnsAsync(coordinateEntity);

        _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0);

        var expectedError = Errors_Common.FailedToDelete.FormatWith("streetcodeCoordinate");

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Errors.First().Message.Should().Contain(expectedError);
        _mockStreetcodeCoordinateRepository.Verify(r => r.GetFirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<StreetcodeCoordinate, bool>>>(), null), Times.Once);
        _mockStreetcodeCoordinateRepository.Verify(r => r.Delete(It.IsAny<StreetcodeCoordinate>()), Times.Once);
        _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}