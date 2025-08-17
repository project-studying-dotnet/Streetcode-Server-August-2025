using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Media.Art;
using Streetcode.BLL.DTO.Media.Images;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Media.Art.GetByStreetcodeId;
using Image = Streetcode.DAL.Entities.Media.Images.Image;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;
using static System.Net.Mime.MediaTypeNames;
using ArtEntity = Streetcode.DAL.Entities.Media.Images.Art;

namespace Streetcode.XUnitTest.BLL_Tests.MediatR.Media.Art.GetByStreetcodeId
{
    public class GetArtsByStreetcodeIdHandlerTests
    {
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IBlobService> _blobServiceMock;
        private readonly GetArtsByStreetcodeIdHandler _handler;

        public GetArtsByStreetcodeIdHandlerTests()
        {
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILoggerService>();
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _blobServiceMock = new Mock<IBlobService>();
            _handler = new GetArtsByStreetcodeIdHandler(
                _repositoryWrapperMock.Object,
                _mapperMock.Object,
                _blobServiceMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task WhenArtsExist_ReturnsOk()
        {
            const int artId = 1;
            const int imageId = 1;
            const string blobName = "Blob1.jpeg";
            const string base64 = "image";
            const int streetcodeId = 1;

            var image = new Image { Id = imageId, BlobName = blobName };
            var imageDto = new ImageDTO { Id = imageId, BlobName = blobName, Base64 = base64 };

            var art = new ArtEntity { Id = artId, Image = image, ImageId = image.Id };
            var artDto = new ArtDTO { Id = artId, Image = imageDto, ImageId = imageDto.Id };

            _repositoryWrapperMock
                .Setup(r => r.ArtRepository.GetAllAsync(
                    It.IsAny<Expression<Func<ArtEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<ArtEntity>, IIncludableQueryable<ArtEntity, object>>>()))
                .ReturnsAsync(new List<ArtEntity> { art });

            _mapperMock
                .Setup(m => m.Map<IEnumerable<ArtDTO>>(It.IsAny<IEnumerable<ArtEntity>>()))
                .Returns(new List<ArtDTO> { artDto });

            _blobServiceMock
                .Setup(b => b.FindFileInStorageAsBase64(blobName))
                .Returns(base64);

            var query = new GetArtsByStreetcodeIdQuery(streetcodeId);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            var returnedArt = Assert.Single(result.Value);
            Assert.Equal(new List<ArtDTO> { artDto }, result.Value);
            Assert.Equal(base64, returnedArt.Image.Base64);

            _repositoryWrapperMock.Verify(r => r.ArtRepository.GetAllAsync(
                It.IsAny<Expression<Func<ArtEntity, bool>>>(),
                It.IsAny<Func<IQueryable<ArtEntity>, IIncludableQueryable<ArtEntity, object>>>()), Times.Once);

            _mapperMock.Verify(m => m.Map<IEnumerable<ArtDTO>>(It.IsAny<IEnumerable<ArtEntity>>()), Times.Once);
            _blobServiceMock.Verify(b => b.FindFileInStorageAsBase64(blobName), Times.Once);
            _loggerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task WhenArtsListIsNull_ReturnsFailAndLogsError()
        {
            const int streetcodeId = 1;
            var query = new GetArtsByStreetcodeIdQuery(streetcodeId);
            var message = $"Cannot find any art with corresponding streetcode id: {streetcodeId}";

            _repositoryWrapperMock
                .Setup(r => r.ArtRepository.GetAllAsync(
                    It.IsAny<Expression<Func<ArtEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<ArtEntity>, IIncludableQueryable<ArtEntity, object>>>()))
                .ReturnsAsync((List<ArtEntity>)null);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(message, result.Errors[0].Message);
            _loggerMock.Verify(l => l.LogError(query, message), Times.Once);

            _repositoryWrapperMock.Verify(r => r.ArtRepository.GetAllAsync(
                It.IsAny<Expression<Func<ArtEntity, bool>>>(),
                It.IsAny<Func<IQueryable<ArtEntity>, IIncludableQueryable<ArtEntity, object>>>()), Times.Once);

            _mapperMock.VerifyNoOtherCalls();
            _blobServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task WhenStreetcodeDoesNotExist_ReturnsFailAndLogsError()
        {
            int artId = 1;
            const int nonExistentStreetcodeId = -1;

            var art = new ArtEntity { Id = artId };
            var query = new GetArtsByStreetcodeIdQuery(nonExistentStreetcodeId);
            var message = $"Cannot find any art with corresponding streetcode id: {nonExistentStreetcodeId}";

            _repositoryWrapperMock
                .Setup(r => r.ArtRepository.GetAllAsync(
                    It.IsAny<Expression<Func<ArtEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<ArtEntity>, IIncludableQueryable<ArtEntity, object>>>()))
                .ReturnsAsync(new List<ArtEntity> { art });

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(message, result.Errors[0].Message);
            _loggerMock.Verify(l => l.LogError(query, message), Times.Once);

            _repositoryWrapperMock.Verify(r => r.ArtRepository.GetAllAsync(
                It.IsAny<Expression<Func<ArtEntity, bool>>>(),
                It.IsAny<Func<IQueryable<ArtEntity>, IIncludableQueryable<ArtEntity, object>>>()), Times.Once);

            _mapperMock.VerifyNoOtherCalls();
            _blobServiceMock.VerifyNoOtherCalls();
        }
    }
}
