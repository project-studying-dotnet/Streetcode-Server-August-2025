namespace Streetcode.XUnitTest.BLL.MediatR.Media.Audio;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using Moq;
using Streetcode.BLL.MediatR.Media.Audio.GetBaseAudio;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Entities.Media;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

public class GetBaseAudioHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepo;
    private readonly Mock<IBlobService> _mockBlobService;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly GetBaseAudioHandler _handler;

    public GetBaseAudioHandlerTests()
    {
        _mockRepo = new Mock<IRepositoryWrapper>();
        _mockBlobService = new Mock<IBlobService>();
        _mockLogger = new Mock<ILoggerService>();

        _handler = new GetBaseAudioHandler(_mockBlobService.Object, _mockRepo.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetBaseAudio_ReturnsMemoryStream_WhenAudioExists()
    {
        var audioId = 1;
        var audioEntity = new Audio
        {
            Id = audioId,
            BlobName = "test.mp3",
        };

        var memoryStream = new MemoryStream(new byte[] { 1, 2, 3 });

        _mockRepo.Setup(r => r.AudioRepository.GetFirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Audio, bool>>>(),
                null))
            .ReturnsAsync(audioEntity);

        _mockBlobService.Setup(b => b.FindFileInStorageAsMemoryStream("test.mp3"))
            .Returns(memoryStream);

        var query = new GetBaseAudioQuery(audioId);
        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(memoryStream, result.Value);
    }

    [Fact]
    public async Task GetBaseAudio_ReturnsFailure_WhenAudioDoesNotExist()
    {
        var audioId = 1;

        _mockRepo.Setup(r => r.AudioRepository.GetFirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Audio, bool>>>(),
                null))
            .ReturnsAsync((Audio)null);

        var query = new GetBaseAudioQuery(audioId);

        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsFailed);
        _mockLogger.Verify(l => l.LogError(query, It.IsAny<string>()), Times.Once);
    }
}
