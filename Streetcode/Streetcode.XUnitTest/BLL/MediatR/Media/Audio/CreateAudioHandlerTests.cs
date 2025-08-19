namespace Streetcode.XUnitTest.BLL.MediatR.Media.Audio;
using AutoMapper;
using FluentResults;
using Moq;
using Streetcode.BLL.DTO.Media.Audio;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Media.Audio.Create;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

using DalAudio = DAL.Entities.Media.Audio;

public class CreateAudioHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnSuccessResult_WhenAudioCreated()
    {
        // Arrange
        var mockBlobService = new Mock<IBlobService>();
        mockBlobService.Setup(x => x.SaveFileInStorage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns("hash123");

        var mockAudioRepo = new Mock<IRepositoryWrapper>();
        var mockAudioDbSet = new Mock<IRepositoryWrapper>();

        var audioEntity = new DalAudio
        {
            Id = 1,
            BlobName = "hash123.mp3",
        };
        mockAudioRepo.Setup(x => x.AudioRepository.CreateAsync(It.IsAny<DalAudio>()))
            .ReturnsAsync((DalAudio a) => a);

        mockAudioRepo.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        var mockMapper = new Mock<IMapper>();
        mockMapper.Setup(m => m.Map<DalAudio>(It.IsAny<AudioFileBaseCreateDTO>()))
            .Returns(audioEntity);

        mockMapper.Setup(m => m.Map<AudioDTO>(It.IsAny<DalAudio>()))
            .Returns(new AudioDTO
            {
                Id = 1,
                BlobName = "hash123.mp3",
                Base64 = "base64string",
                MimeType = "audio/mpeg",
            });

        var mockLogger = new Mock<ILoggerService>();

        var handler = new CreateAudioHandler(
            mockBlobService.Object,
            mockAudioRepo.Object,
            mockMapper.Object,
            mockLogger.Object);

        var command = new CreateAudioCommand(new AudioFileBaseCreateDTO
        {
            BaseFormat = "base64string",
            Title = "testAudio",
            Extension = "mp3",
        });

        var result = await handler.Handle(command, default);

        Assert.True(result.IsSuccess);
        Assert.Equal("hash123.mp3", result.Value.BlobName);
    }
}
