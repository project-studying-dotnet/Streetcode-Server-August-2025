namespace Streetcode.XUnitTest.BLL.MediatR.Media.Audio;
using AutoMapper;
using FluentResults;
using Moq;
using Streetcode.BLL.DTO.Media.Audio;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Media.Audio.GetByStreetcodeId;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;

using AudioEntity = DAL.Entities.Media.Audio;

public class GetAudioByStreetcodeIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnAudioDTO_WhenAudioExists()
    {
        // Arrange
        var streetcodeId = 1;

        var audioEntity = new AudioEntity
        {
            Id = 1,
            BlobName = "file.mp3",
        };

        var streetcodeEntity = new StreetcodeContent
        {
            Id = streetcodeId,
            Audio = audioEntity,
        };

        var audioDto = new AudioDTO
        {
            Id = 1,
            BlobName = "file.mp3",
            Base64 = "base64string",
        };

        var repositoryMock = new Mock<IRepositoryWrapper>();
        var mapperMock = new Mock<IMapper>();
        var blobServiceMock = new Mock<IBlobService>();
        var loggerMock = new Mock<ILoggerService>();

        repositoryMock.Setup(r => r.StreetcodeRepository.GetFirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<StreetcodeContent, bool>>>(),
            It.IsAny<Func<IQueryable<StreetcodeContent>, IIncludableQueryable<StreetcodeContent, object>>>()))
            .ReturnsAsync(streetcodeEntity);

        mapperMock.Setup(m => m.Map<AudioDTO>(audioEntity)).Returns(audioDto);

        blobServiceMock.Setup(b => b.FindFileInStorageAsBase64(audioDto.BlobName))
            .Returns(audioDto.Base64);

        var handler = new GetAudioByStreetcodeIdQueryHandler(
            repositoryMock.Object,
            mapperMock.Object,
            blobServiceMock.Object,
            loggerMock.Object);

        var query = new GetAudioByStreetcodeIdQuery(streetcodeId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(audioDto.Id, result.Value.Id);
        Assert.Equal(audioDto.BlobName, result.Value.BlobName);
        Assert.Equal(audioDto.Base64, result.Value.Base64);

        mapperMock.Verify(m => m.Map<AudioDTO>(audioEntity), Times.Exactly(2));
    }
}
