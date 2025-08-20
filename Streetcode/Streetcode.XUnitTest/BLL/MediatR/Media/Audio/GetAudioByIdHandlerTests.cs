namespace Streetcode.XUnitTest.BLL.MediatR.Media.Audio;
using AutoMapper;
using FluentResults;
using Moq;
using Streetcode.BLL.DTO.Media.Audio;
using Streetcode.BLL.MediatR.Media.Audio.GetById;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Entities.Media;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;
using System.Linq.Expressions;

public class GetAudioByIdHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _repositoryMock;
    private readonly Mock<IBlobService> _blobServiceMock;
    private readonly Mock<ILoggerService> _loggerMock;
    private readonly IMapper _mapper;

    public GetAudioByIdHandlerTests()
    {
        _repositoryMock = new Mock<IRepositoryWrapper>();
        _blobServiceMock = new Mock<IBlobService>();
        _loggerMock = new Mock<ILoggerService>();

        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Audio, AudioDTO>();
        });
        _mapper = config.CreateMapper();
    }

    [Fact]
    public async Task GetAudioById_ReturnsAudioDTO_WhenAudioExists()
    {
        int audioId = 1;

        var audioEntity = new Audio
        {
            Id = audioId,
            BlobName = "test.mp3",
        };

        _repositoryMock.Setup(r => r.AudioRepository.GetFirstOrDefaultAsync(
It.IsAny<Expression<Func<Audio, bool>>>(),
null))
.ReturnsAsync(audioEntity);

        _blobServiceMock
            .Setup(b => b.FindFileInStorageAsBase64(It.IsAny<string>()))
            .Returns("base64string");

        var handler = new GetAudioByIdHandler(
            _repositoryMock.Object,
            _mapper,
            _blobServiceMock.Object,
            _loggerMock.Object);

        var query = new GetAudioByIdQuery(audioId);

        var result = await handler.Handle(query, default);

        Assert.True(result.IsSuccess);
        Assert.Equal(audioId, result.Value.Id);
        Assert.Equal("test.mp3", result.Value.BlobName);
        Assert.Equal("base64string", result.Value.Base64);
    }

    [Fact]
    public async Task GetAudioById_ReturnsFail_WhenAudioDoesNotExist()
    {
        _repositoryMock.Setup(r => r.AudioRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<Audio, bool>>>(),
                null))
            .ReturnsAsync((Audio?)null);

        var handler = new GetAudioByIdHandler(
            _repositoryMock.Object,
            _mapper,
            _blobServiceMock.Object,
            _loggerMock.Object);

        var query = new GetAudioByIdQuery(1);
        var result = await handler.Handle(query, default);

        Assert.True(result.IsFailed);
    }
}
