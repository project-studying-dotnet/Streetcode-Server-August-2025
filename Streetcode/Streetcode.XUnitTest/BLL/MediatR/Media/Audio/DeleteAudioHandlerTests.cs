namespace Streetcode.XUnitTest.BLL.MediatR.Media.Audio;
using Moq;
using FluentAssertions;
using FluentResults;
using MediatR;
using Streetcode.BLL.MediatR.Media.Audio.Delete;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Threading.Tasks;
using System.Threading;
using Xunit;
using global::MediatR;

public class DeleteAudioHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepoWrapper;
    private readonly Mock<IBlobService> _mockBlobService;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly DeleteAudioHandler _handler;

    public DeleteAudioHandlerTests()
    {
        _mockRepoWrapper = new Mock<IRepositoryWrapper>();
        _mockBlobService = new Mock<IBlobService>();
        _mockLogger = new Mock<ILoggerService>();

        _handler = new DeleteAudioHandler(
            _mockRepoWrapper.Object,
            _mockBlobService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task DeleteAudio_ShouldReturnSuccess_WhenAudioDeleted()
    {
        var audio = new DAL.Entities.Media.Audio
        {
            Id = 1,
            BlobName = "test.mp3",
        };

        _mockRepoWrapper.Setup(r => r.AudioRepository.GetFirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<DAL.Entities.Media.Audio, bool>>>(),
            null))
            .ReturnsAsync(audio);

        _mockRepoWrapper.Setup(r => r.AudioRepository.Delete(audio));
        _mockRepoWrapper.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        var result = await _handler.Handle(new DeleteAudioCommand(audio.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(Unit.Value);

        _mockRepoWrapper.Verify(r => r.AudioRepository.Delete(audio), Times.Once);
        _mockRepoWrapper.Verify(r => r.SaveChangesAsync(), Times.Once);
        _mockBlobService.Verify(b => b.DeleteFileInStorage(audio.BlobName), Times.Once);
        _mockLogger.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Once);
    }
}
