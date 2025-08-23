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
using System.Reflection.Metadata;

public class GetAudioByStreetcodeIdQueryHandlerTests
{
        private readonly Mock<IRepositoryWrapper> _repositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IBlobService> _blobServiceMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly GetAudioByStreetcodeIdQueryHandler _handler;

        public GetAudioByStreetcodeIdQueryHandlerTests()
    {
        _repositoryMock = new Mock<IRepositoryWrapper>();
        _mapperMock = new Mock<IMapper>();
        _blobServiceMock = new Mock<IBlobService>();
        _loggerMock = new Mock<ILoggerService>();
        _handler = new GetAudioByStreetcodeIdQueryHandler(
            _repositoryMock.Object,
            _mapperMock.Object,
            _blobServiceMock.Object,
            _loggerMock.Object);
    }

        [Fact]
        public async Task GetAudioByStreetcodeId_ShouldReturnAudioDTO_WhenAudioExists()
    {
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
        _repositoryMock.Setup(r => r.StreetcodeRepository.GetFirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<StreetcodeContent, bool>>>(),
            It.IsAny<Func<IQueryable<StreetcodeContent>, IIncludableQueryable<StreetcodeContent, object>>>()))
            .ReturnsAsync(streetcodeEntity);

        _mapperMock.Setup(m => m.Map<AudioDTO>(audioEntity)).Returns(audioDto);

        _blobServiceMock.Setup(b => b.FindFileInStorageAsBase64(audioDto.BlobName))
            .Returns(audioDto.Base64);

        var query = new GetAudioByStreetcodeIdQuery(streetcodeId);

        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(audioDto.Id, result.Value.Id);
        Assert.Equal(audioDto.BlobName, result.Value.BlobName);
        Assert.Equal(audioDto.Base64, result.Value.Base64);

        _mapperMock.Verify(m => m.Map<AudioDTO>(audioEntity), Times.Exactly(2));
    }
}
