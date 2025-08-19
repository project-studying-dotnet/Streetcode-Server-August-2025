using System.Linq.Expressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Media.Art;
using Streetcode.BLL.DTO.Media.Images;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Media.Art.GetByStreetcodeId;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;
using ArtEntity = Streetcode.DAL.Entities.Media.Images.Art;
using Image = Streetcode.DAL.Entities.Media.Images.Image;

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
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_WhenArtsExist_ReturnsAllWithBase64()
        {
            // Arrange
            const int StreetcodeId = 1;
            const string BlobName1 = "Blob1.jpeg";
            const string BlobName2 = "Blob2.jpeg";
            const string Base64Value = "base64-image";

            var arts = new List<ArtEntity>
            {
                new ArtEntity { Id = 1, Image = new Image { Id = 1, BlobName = BlobName1 }, ImageId = 1 },
                new ArtEntity { Id = 2, Image = new Image { Id = 2, BlobName = BlobName2 }, ImageId = 2 },
            };

            var artsDto = arts.Select(a => new ArtDTO
            {
                Id = a.Id,
                ImageId = a.ImageId,
                Image = new ImageDTO { Id = a.ImageId, BlobName = a.Image.BlobName, Base64 = Base64Value },
            }).ToList();

            _repositoryWrapperMock
                .Setup(r => r.ArtRepository.GetAllAsync(
                    It.IsAny<Expression<Func<ArtEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<ArtEntity>, IIncludableQueryable<ArtEntity, object>>>()))
                .ReturnsAsync(arts);

            _mapperMock
                .Setup(m => m.Map<IEnumerable<ArtDTO>>(It.IsAny<IEnumerable<ArtEntity>>()))
                .Returns(artsDto);

            _blobServiceMock
                .Setup(b => b.FindFileInStorageAsBase64(It.IsAny<string>()))
                .Returns(Base64Value);

            var query = new GetArtsByStreetcodeIdQuery(StreetcodeId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.Count());
            Assert.All(result.Value, a => Assert.Equal(Base64Value, a.Image.Base64));

            _blobServiceMock.Verify(b => b.FindFileInStorageAsBase64(It.IsAny<string>()), Times.Exactly(2));
        }

        [Fact]
        public async Task Handle_WhenArtsListIsEmpty_ReturnsSuccess()
        {
            // Arrange
            const int StreetcodeId = 1;

            _repositoryWrapperMock
                .Setup(r => r.ArtRepository.GetAllAsync(
                    It.IsAny<Expression<Func<ArtEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<ArtEntity>, IIncludableQueryable<ArtEntity, object>>>()))
                .ReturnsAsync(new List<ArtEntity>());

            _mapperMock
                .Setup(m => m.Map<IEnumerable<ArtDTO>>(It.IsAny<IEnumerable<ArtEntity>>()))
                .Returns(new List<ArtDTO>());

            var query = new GetArtsByStreetcodeIdQuery(StreetcodeId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Empty(result.Value);
            _blobServiceMock.Verify(b => b.FindFileInStorageAsBase64(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenArtsListIsNull_ReturnsFail()
        {
            // Arrange
            const int StreetcodeId = 1;
            var query = new GetArtsByStreetcodeIdQuery(StreetcodeId);
            var expectedMessage = $"Cannot find any art with corresponding streetcode id: {StreetcodeId}";

            _repositoryWrapperMock
                .Setup(r => r.ArtRepository.GetAllAsync(
                    It.IsAny<Expression<Func<ArtEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<ArtEntity>, IIncludableQueryable<ArtEntity, object>>>()))
                .ReturnsAsync((List<ArtEntity>)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(expectedMessage, result.Errors.First().Message);
            _loggerMock.Verify(l => l.LogError(query, expectedMessage), Times.Once);
            _blobServiceMock.Verify(b => b.FindFileInStorageAsBase64(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenArtsHaveNoImage_ReturnsSuccessWithoutBlobLoading()
        {
            // Arrange
            const int ArtId = 1;

            var art = new ArtEntity { Id = ArtId, Image = null };
            var artDto = new ArtDTO { Id = ArtId, Image = null };

            _repositoryWrapperMock
                .Setup(r => r.ArtRepository.GetAllAsync(
                    It.IsAny<Expression<Func<ArtEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<ArtEntity>, IIncludableQueryable<ArtEntity, object>>>()))
                .ReturnsAsync(new List<ArtEntity> { art });

            _mapperMock
                .Setup(m => m.Map<IEnumerable<ArtDTO>>(It.IsAny<IEnumerable<ArtEntity>>()))
                .Returns(new List<ArtDTO> { artDto });

            var query = new GetArtsByStreetcodeIdQuery(1);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(new List<ArtDTO> { artDto }, result.Value);
            _blobServiceMock.Verify(b => b.FindFileInStorageAsBase64(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenImageBlobNameIsNull_ReturnsSuccessSkippingBlobLoading()
        {
            // Arrange
            const int ArtId = 1;
            const int ImageId = 2;

            var art = new ArtEntity { Id = ArtId, Image = new Image { Id = ImageId, BlobName = null }, ImageId = ImageId };
            var artDto = new ArtDTO { Id = ArtId, Image = new ImageDTO { Id = ImageId, BlobName = null }, ImageId = ImageId };

            _repositoryWrapperMock
                .Setup(r => r.ArtRepository.GetAllAsync(
                    It.IsAny<Expression<Func<ArtEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<ArtEntity>, IIncludableQueryable<ArtEntity, object>>>()))
                .ReturnsAsync(new List<ArtEntity> { art });

            _mapperMock
                .Setup(m => m.Map<IEnumerable<ArtDTO>>(It.IsAny<IEnumerable<ArtEntity>>()))
                .Returns(new List<ArtDTO> { artDto });

            var query = new GetArtsByStreetcodeIdQuery(1);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(new List<ArtDTO> { artDto }, result.Value);
            _blobServiceMock.Verify(b => b.FindFileInStorageAsBase64(It.IsAny<string>()), Times.Never);
        }
    }
}
