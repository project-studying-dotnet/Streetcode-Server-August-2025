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
    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenAudioDeleted()
    {
        // Arrange
        var mockRepoWrapper = new Mock<IRepositoryWrapper>();
        var mockBlobService = new Mock<IBlobService>();
        var mockLogger = new Mock<ILoggerService>();

        var audio = new DAL.Entities.Media.Audio
        {
            Id = 1,
            BlobName = "test.mp3",
        };

        
        mockRepoWrapper.Setup(r => r.AudioRepository.GetFirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<DAL.Entities.Media.Audio, bool>>>(),
            null))
            .ReturnsAsync(audio);

        
        mockRepoWrapper.Setup(r => r.AudioRepository.Delete(audio));

        
        mockRepoWrapper.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        var handler = new DeleteAudioHandler(
            mockRepoWrapper.Object,
            mockBlobService.Object,
            mockLogger.Object);

        var command = new DeleteAudioCommand(audio.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(Unit.Value);

        mockRepoWrapper.Verify(r => r.AudioRepository.Delete(audio), Times.Once);
        mockRepoWrapper.Verify(r => r.SaveChangesAsync(), Times.Once);
        mockBlobService.Verify(b => b.DeleteFileInStorage(audio.BlobName), Times.Once);
        mockLogger.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Once);
    }
}
