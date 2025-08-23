namespace Streetcode.XUnitTest.BLL.MediatR.Media.Audio;
using AutoMapper;
using FluentResults;
using Moq;
using Streetcode.BLL.DTO.Media.Audio;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Media.Audio.GetAll;
using Streetcode.DAL.Entities.Media;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using System;

public class GetAllAudioHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepo;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IBlobService> _mockBlobService;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly GetAllAudiosHandler _handler;

    public GetAllAudioHandlerTests()
    {
        _mockRepo = new Mock<IRepositoryWrapper>();
        _mockMapper = new Mock<IMapper>();
        _mockBlobService = new Mock<IBlobService>();
        _mockLogger = new Mock<ILoggerService>();

        _handler = new GetAllAudiosHandler(
            _mockRepo.Object,
            _mockMapper.Object,
            _mockBlobService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task GetAllAudio_ShouldReturnAudioDtos_WhenAudiosExist()
    {
        var audiosFromDb = new List<Audio>
            {
                new Audio { Id = 1, BlobName = "file1.mp3" },
                new Audio { Id = 2, BlobName = "file2.mp3" }
            };

        var audioDtos = new List<AudioDTO>
            {
                new AudioDTO { Id = 1, BlobName = "file1.mp3" },
                new AudioDTO { Id = 2, BlobName = "file2.mp3" }
            };

        _mockRepo.Setup(r => r.AudioRepository.GetAllAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Audio, bool>>>(),
            It.IsAny<Func<IQueryable<Audio>, IIncludableQueryable<Audio, object>>>()))
            .ReturnsAsync(audiosFromDb);

        _mockMapper.Setup(m => m.Map<IEnumerable<AudioDTO>>(audiosFromDb))
            .Returns(audioDtos);

        _mockBlobService.Setup(b => b.FindFileInStorageAsBase64(It.IsAny<string>()))
            .Returns<string>(name => Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(name)));

        var result = await _handler.Handle(new GetAllAudiosQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count());
        Assert.All(result.Value, dto => Assert.NotNull(dto.Base64));
    }

    [Fact]
    public async Task GetAllAudio_ShouldReturnFail_WhenNoAudiosExist()
    {
        _mockRepo.Setup(r => r.AudioRepository.GetAllAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Audio, bool>>>(),
            It.IsAny<Func<IQueryable<Audio>, IIncludableQueryable<Audio, object>>>()))
            .ReturnsAsync((IEnumerable<Audio>?)null);

        var result = await _handler.Handle(new GetAllAudiosQuery(), CancellationToken.None);

        Assert.True(result.IsFailed);
        _mockLogger.Verify(l => l.LogError(It.IsAny<GetAllAudiosQuery>(), "Cannot find any audios"), Times.Once);
    }
}
