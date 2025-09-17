using AutoMapper;
using FluentAssertions;
using FluentResults;
using MediatR;
using Moq;
using Streetcode.BLL.DTO.AdditionalContent;
using Streetcode.BLL.DTO.AdditionalContent.Tag;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.AdditionalContent.Tag.Create;
using Streetcode.DAL.Entities.AdditionalContent;
using Streetcode.DAL.Repositories.Interfaces.AdditionalContent;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.AdditionalContent.Tag;

public class CreateTagHandlerTests
{
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly Mock<ITagRepository> _mockTagRepository;
    private readonly CreateTagHandler _handler;

    public CreateTagHandlerTests()
    {
        _mockMapper = new Mock<IMapper>();
        _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
        _mockLogger = new Mock<ILoggerService>();
        _mockTagRepository = new Mock<ITagRepository>();
        _mockRepositoryWrapper.Setup(x => x.TagRepository).Returns(_mockTagRepository.Object);
        _handler = new CreateTagHandler(_mockRepositoryWrapper.Object, _mockMapper.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_WhenTagIsCreatedSuccessfully_ShouldReturnSuccess()
    {
        // Arrange
        var request = new CreateTagQuery(new CreateTagDTO { Title = "Test Tag" });
        var tagEntity = new DAL.Entities.AdditionalContent.Tag { Id = 1, Title = "Test Tag" };
        var expectedDto = new TagDTO { Id = 1, Title = "Test Tag" };

        _mockTagRepository.Setup(r => r.CreateAsync(It.IsAny<DAL.Entities.AdditionalContent.Tag>()))
            .ReturnsAsync(tagEntity);
        _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
        _mockMapper.Setup(m => m.Map<TagDTO>(tagEntity)).Returns(expectedDto);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedDto);
        _mockTagRepository.Verify(r => r.CreateAsync(It.IsAny<DAL.Entities.AdditionalContent.Tag>()), Times.Once);
        _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenSaveChangesAsyncThrowsException_ShouldReturnFailure()
    {
        // Arrange
        var request = new CreateTagQuery(new CreateTagDTO { Title = "Test Tag" });
        var tagEntity = new DAL.Entities.AdditionalContent.Tag { Id = 1, Title = "Test Tag" };
        var exception = new Exception("Database save failed.");

        _mockTagRepository.Setup(r => r.CreateAsync(It.IsAny<DAL.Entities.AdditionalContent.Tag>()))
            .ReturnsAsync(tagEntity);
        _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync()).ThrowsAsync(exception);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Errors.First().Message.Should().Be(exception.ToString());
        _mockTagRepository.Verify(r => r.CreateAsync(It.IsAny<DAL.Entities.AdditionalContent.Tag>()), Times.Once);
        _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Once);
        _mockLogger.Verify(l => l.LogError(It.IsAny<CreateTagQuery>(), exception.ToString()), Times.Once);
    }
}