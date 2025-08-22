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
    private readonly Mock<IBlobService> _mockBlobService;
    private readonly Mock<IRepositoryWrapper> _mockRepo;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly CreateAudioHandler _handler;

    public CreateAudioHandlerTests()
    {
        _mockBlobService = new Mock<IBlobService>();
        _mockRepo = new Mock<IRepositoryWrapper>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILoggerService>();

        _handler = new CreateAudioHandler(
            _mockBlobService.Object,
            _mockRepo.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task CreateAudio_ShouldReturnSuccessResult_WhenAudioCreated()
    {
        var audioEntity = new DalAudio
        {
            Id = 1,
            BlobName = "hash123.mp3",
        };

        _mockBlobService
            .Setup(x => x.SaveFileInStorage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns("hash123");

        _mockRepo
            .Setup(x => x.AudioRepository.CreateAsync(It.IsAny<DalAudio>()))
            .ReturnsAsync((DalAudio a) => a);

        _mockRepo
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        _mockMapper
            .Setup(m => m.Map<DalAudio>(It.IsAny<AudioFileBaseCreateDTO>()))
            .Returns(audioEntity);

        _mockMapper
            .Setup(m => m.Map<AudioDTO>(It.IsAny<DalAudio>()))
            .Returns(new AudioDTO
            {
                Id = 1,
                BlobName = "hash123.mp3",
                Base64 = "base64string",
                MimeType = "audio/mpeg",
            });

        var command = new CreateAudioCommand(new AudioFileBaseCreateDTO
        {
            BaseFormat = "base64string",
            Title = "testAudio",
            Extension = "mp3",
        });

        var result = await _handler.Handle(command, default);

        Assert.True(result.IsSuccess);
        Assert.Equal("hash123.mp3", result.Value.BlobName);
    }
}