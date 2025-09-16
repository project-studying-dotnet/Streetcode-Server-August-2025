using AutoMapper;
using FluentAssertions;
using FluentResults;
using MediatR;
using Moq;
using Streetcode.BLL.MediatR.AdditionalContent.Coordinate.Create;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.DAL.Entities.AdditionalContent.Coordinates.Types;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.AdditionalContent.Coordinate;

public class CreateCoordinateHandlerTests
{
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
    private readonly Mock<IMediator> _mockMediator; // Added for completeness, if used in other handlers
    private readonly CreateCoordinateHandler _handler;

    public CreateCoordinateHandlerTests()
    {
        _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
        _mockMapper = new Mock<IMapper>();
        _mockMediator = new Mock<IMediator>();

        // We can mock specific repositories as needed, here a fake one for StreetcodeCoordinate
        _mockRepositoryWrapper.Setup(x => x.StreetcodeCoordinateRepository)
            .Returns(Mock.Of<Streetcode.DAL.Repositories.Interfaces.AdditionalContent.IStreetcodeCoordinateRepository>());

        _handler = new CreateCoordinateHandler(
            _mockRepositoryWrapper.Object,
            _mockMapper.Object);
    }

    [Fact]
    public async Task Handle_WhenCoordinateIsCreatedSuccessfully_ShouldReturnSuccess()
    {
        // Arrange
        var request = new CreateCoordinateCommand(new Streetcode.BLL.DTO.AdditionalContent.Coordinates.Types.StreetcodeCoordinateDTO());
        var mappedEntity = new StreetcodeCoordinate();

        _mockMapper.Setup(m => m.Map<StreetcodeCoordinate>(It.IsAny<Streetcode.BLL.DTO.AdditionalContent.Coordinates.Types.StreetcodeCoordinateDTO>()))
            .Returns(mappedEntity);

        _mockRepositoryWrapper.Setup(x => x.StreetcodeCoordinateRepository.CreateAsync(mappedEntity))
            .ReturnsAsync(mappedEntity);

        _mockRepositoryWrapper.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(Unit.Value);
        _mockRepositoryWrapper.Verify(x => x.StreetcodeCoordinateRepository.CreateAsync(It.IsAny<StreetcodeCoordinate>()), Times.Once);
        _mockRepositoryWrapper.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenSaveChangesFails_ShouldReturnFailure()
    {
        // Arrange
        var request = new CreateCoordinateCommand(new Streetcode.BLL.DTO.AdditionalContent.Coordinates.Types.StreetcodeCoordinateDTO());
        var mappedEntity = new StreetcodeCoordinate();

        _mockMapper.Setup(m => m.Map<StreetcodeCoordinate>(It.IsAny<Streetcode.BLL.DTO.AdditionalContent.Coordinates.Types.StreetcodeCoordinateDTO>()))
            .Returns(mappedEntity);

        _mockRepositoryWrapper.Setup(x => x.StreetcodeCoordinateRepository.CreateAsync(mappedEntity))
            .ReturnsAsync(mappedEntity);

        _mockRepositoryWrapper.Setup(x => x.SaveChangesAsync()).ReturnsAsync(0);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Errors.First().Message.Should().Contain(Errors_Common.FailedToCreate.FormatWith("streetcodeCoordinate"));
        _mockRepositoryWrapper.Verify(x => x.StreetcodeCoordinateRepository.CreateAsync(It.IsAny<StreetcodeCoordinate>()), Times.Once);
        _mockRepositoryWrapper.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenMappingReturnsNull_ShouldReturnFailure()
    {
        // Arrange
        var request = new CreateCoordinateCommand(new Streetcode.BLL.DTO.AdditionalContent.Coordinates.Types.StreetcodeCoordinateDTO());

        _mockMapper.Setup(m => m.Map<StreetcodeCoordinate>(It.IsAny<Streetcode.BLL.DTO.AdditionalContent.Coordinates.Types.StreetcodeCoordinateDTO>()))
            .Returns((StreetcodeCoordinate)null);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Errors.First().Message.Should().Contain(Errors_Common.CannotConvertNull.FormatWith("streetcodeCoordinate"));
        _mockRepositoryWrapper.Verify(x => x.StreetcodeCoordinateRepository.CreateAsync(It.IsAny<StreetcodeCoordinate>()), Times.Never);
        _mockRepositoryWrapper.Verify(x => x.SaveChangesAsync(), Times.Never);
    }
}