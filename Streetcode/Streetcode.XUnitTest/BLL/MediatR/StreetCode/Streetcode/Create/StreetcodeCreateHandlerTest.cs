using AutoMapper;
using FluentResults;
using MediatR;
using Moq;
using Streetcode.BLL.DTO.Streetcode;
using Streetcode.BLL.DTO.Streetcode.Create;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.Streetcode.Create;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.MediatRTests.Streetcode.Create;

public class StreetcodeCreateHandlerTest
{
    private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly StreetcodeCreateHandler _handler;

    public StreetcodeCreateHandlerTest()
    {
        _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILoggerService>();
        _handler = new StreetcodeCreateHandler(
            _mockRepositoryWrapper.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsSuccessResult()
    {
        var newDTO = CreateValidDTO();
        var streetcodeEntity = new StreetcodeContent { Id = 1 };
        var streetcodeDto = new StreetcodeDTO { Id = 1 };
        var request = new StreetcodeCreateCommand(newDTO);

        _mockMapper.Setup(m => m.Map<StreetcodeContent>(request.newStreetcode))
            .Returns(streetcodeEntity);

        _mockRepositoryWrapper.Setup(r => r.StreetcodeRepository.Create(streetcodeEntity));
        _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        _mockMapper.Setup(m => m.Map<StreetcodeDTO>(streetcodeEntity))
            .Returns(streetcodeDto);

        var result = await _handler.Handle(request, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(1, result.Value.Id);

        _mockRepositoryWrapper.Verify(r => r.StreetcodeRepository.Create(streetcodeEntity), Times.Once);
        _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Once);
        _mockLogger.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Handle_SaveChangesFails_ReturnsFailureResult()
    {
        var newDTO = CreateValidDTO();
        var request = new StreetcodeCreateCommand(newDTO);

        var streetcodeEntity = new StreetcodeContent();

        _mockMapper.Setup(m => m.Map<StreetcodeContent>(request.newStreetcode))
            .Returns(streetcodeEntity);

        _mockRepositoryWrapper.Setup(r => r.StreetcodeRepository.Create(streetcodeEntity));
        _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0);

        var result = await _handler.Handle(request, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Contains("Failed to save streetcode to database", result.Errors[0].Message);

        _mockLogger.Verify(l => l.LogError(request, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Handle_MappingThrowsException_ReturnsFailureResult()
    {
        // Arrange
        var request = new StreetcodeCreateCommand(new StreetcodeCreateDTO());

        _mockMapper.Setup(m => m.Map<StreetcodeContent>(request.newStreetcode))
            .Throws(new AutoMapperMappingException("Mapping failed"));

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Contains("Mapping failed", result.Errors[0].Message);

        _mockLogger.Verify(l => l.LogError(request, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Handle_GeneralException_ReturnsFailureResult()
    {
        var request = new StreetcodeCreateCommand(new StreetcodeCreateDTO());

        _mockMapper.Setup(m => m.Map<StreetcodeContent>(request.newStreetcode))
            .Throws(new Exception("Database connection failed"));

        var result = await _handler.Handle(request, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Contains("Database connection failed", result.Errors[0].Message);

        _mockLogger.Verify(l => l.LogError(request, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidRequest_SetsSystemPropertiesCorrectly()
    {
        var newDTO = CreateValidDTO();
        var request = new StreetcodeCreateCommand(newDTO);

        var streetcodeEntity = new StreetcodeContent();
        var streetcodeDto = new StreetcodeDTO { Id = 1 };

        _mockMapper.Setup(m => m.Map<StreetcodeContent>(request.newStreetcode))
            .Returns(streetcodeEntity);

        _mockRepositoryWrapper.Setup(r => r.StreetcodeRepository.Create(It.IsAny<StreetcodeContent>()))
            .Callback<StreetcodeContent>(entity =>
            {
                Assert.NotEqual(default, entity.CreatedAt);
                Assert.NotEqual(default, entity.UpdatedAt);
                Assert.Equal(0, entity.ViewCount);
            });

        _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        _mockMapper.Setup(m => m.Map<StreetcodeDTO>(It.IsAny<StreetcodeContent>()))
            .Returns(streetcodeDto);

        var result = await _handler.Handle(request, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    private static StreetcodeCreateDTO CreateValidDTO(int id = 1)
    {
        return new StreetcodeCreateDTO
        {
            Title = "Test Streetcode",
            TransliterationUrl = "test-streetcode",
            Index = id,
            Teaser = "Test teaser",
            DateString = "2024"
        };
    }
}