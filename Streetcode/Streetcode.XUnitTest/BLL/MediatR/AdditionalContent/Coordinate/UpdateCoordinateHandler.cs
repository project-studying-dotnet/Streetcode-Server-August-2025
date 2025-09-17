using AutoMapper;
using FluentAssertions;
using FluentResults;
using MediatR;
using Moq;
using Streetcode.BLL.MediatR.AdditionalContent.Coordinate.Update;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.DAL.Entities.AdditionalContent.Coordinates.Types;
using Streetcode.DAL.Repositories.Interfaces.AdditionalContent;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.BLL.DTO.AdditionalContent.Coordinates;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.AdditionalContent.Coordinate;

public class UpdateCoordinateHandlerTests
{
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
    private readonly Mock<IStreetcodeCoordinateRepository> _mockStreetcodeCoordinateRepository;
    private readonly UpdateCoordinateHandler _handler;

    public UpdateCoordinateHandlerTests()
    {
        _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
        _mockMapper = new Mock<IMapper>();
        _mockStreetcodeCoordinateRepository = new Mock<IStreetcodeCoordinateRepository>();

        _mockRepositoryWrapper.Setup(x => x.StreetcodeCoordinateRepository)
            .Returns(_mockStreetcodeCoordinateRepository.Object);

        _handler = new UpdateCoordinateHandler(
            _mockRepositoryWrapper.Object,
            _mockMapper.Object);
    }

    [Fact]
    public async Task Handle_WhenCoordinateIsUpdatedSuccessfully_ShouldReturnSuccess()
    {
        // Arrange
        var request = new UpdateCoordinateCommand(new Streetcode.BLL.DTO.AdditionalContent.Coordinates.Types.StreetcodeCoordinateDTO { Id = 1, Longtitude = 1.0m, Latitude = 2.0m });
        var mappedEntity = new StreetcodeCoordinate { Id = 1, Longtitude = 1.0m, Latitude = 2.0m };

        _mockMapper.Setup(m => m.Map<StreetcodeCoordinate>(It.IsAny<Streetcode.BLL.DTO.AdditionalContent.Coordinates.Types.StreetcodeCoordinateDTO>()))
            .Returns(mappedEntity);

        _mockStreetcodeCoordinateRepository.Setup(r => r.Update(mappedEntity));
        _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(Unit.Value);
        _mockMapper.Verify(m => m.Map<StreetcodeCoordinate>(It.IsAny<Streetcode.BLL.DTO.AdditionalContent.Coordinates.Types.StreetcodeCoordinateDTO>()), Times.Once);
        _mockStreetcodeCoordinateRepository.Verify(r => r.Update(It.IsAny<StreetcodeCoordinate>()), Times.Once);
        _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenSaveChangesFails_ShouldReturnFailure()
    {
        // Arrange
        var request = new UpdateCoordinateCommand(new Streetcode.BLL.DTO.AdditionalContent.Coordinates.Types.StreetcodeCoordinateDTO { Id = 1, Longtitude = 1.0m, Latitude = 2.0m });
        var mappedEntity = new StreetcodeCoordinate { Id = 1, Longtitude = 1.0m, Latitude = 2.0m };

        _mockMapper.Setup(m => m.Map<StreetcodeCoordinate>(It.IsAny<Streetcode.BLL.DTO.AdditionalContent.Coordinates.Types.StreetcodeCoordinateDTO>()))
            .Returns(mappedEntity);

        _mockStreetcodeCoordinateRepository.Setup(r => r.Update(mappedEntity));
        _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0);

        var expectedError = Errors_Common.FailedToUpdate.FormatWith("streetcodeCoordinate");

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Errors.First().Message.Should().Contain(expectedError);
        _mockMapper.Verify(m => m.Map<StreetcodeCoordinate>(It.IsAny<Streetcode.BLL.DTO.AdditionalContent.Coordinates.Types.StreetcodeCoordinateDTO>()), Times.Once);
        _mockStreetcodeCoordinateRepository.Verify(r => r.Update(It.IsAny<StreetcodeCoordinate>()), Times.Once);
        _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenMappingReturnsNull_ShouldReturnFailure()
    {
        // Arrange
        var request = new UpdateCoordinateCommand(new Streetcode.BLL.DTO.AdditionalContent.Coordinates.Types.StreetcodeCoordinateDTO { Id = 1 });
        _mockMapper.Setup(m => m.Map<StreetcodeCoordinate>(It.IsAny<Streetcode.BLL.DTO.AdditionalContent.Coordinates.Types.StreetcodeCoordinateDTO>()))
            .Returns((StreetcodeCoordinate)null);

        var expectedError = Errors_Common.CannotConvertNull.FormatWith("streetcodeCoordinate");

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Errors.First().Message.Should().Contain(expectedError);
        _mockMapper.Verify(m => m.Map<StreetcodeCoordinate>(It.IsAny<Streetcode.BLL.DTO.AdditionalContent.Coordinates.Types.StreetcodeCoordinateDTO>()), Times.Once);
        _mockStreetcodeCoordinateRepository.Verify(r => r.Update(It.IsAny<StreetcodeCoordinate>()), Times.Never);
        _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Never);
    }
}