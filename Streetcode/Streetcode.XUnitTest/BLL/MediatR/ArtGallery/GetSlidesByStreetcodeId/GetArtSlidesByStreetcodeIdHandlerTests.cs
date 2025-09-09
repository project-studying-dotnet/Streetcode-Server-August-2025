using System.Linq.Expressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.ArtGallery;
using Streetcode.BLL.DTO.Media.Art;
using Streetcode.BLL.DTO.Media.Images;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.MediatR.ArtGallery.GetSlidesByStreetcodeId;
using Streetcode.DAL.Entities.Media.Images;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Enums;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.ArtGallery.GetSlidesByStreetcodeId
{
    public class GetArtSlidesByStreetcodeIdHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IBlobService> _blobServiceMock;
        private readonly GetArtSlidesByStreetcodeIdHandler _handler;

        public GetArtSlidesByStreetcodeIdHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _blobServiceMock = new Mock<IBlobService>();
            _handler = new GetArtSlidesByStreetcodeIdHandler(
                _repositoryWrapperMock.Object,
                _mapperMock.Object,
                _blobServiceMock.Object);
        }

        [Fact]
        public async Task Handle_WhenSlidesExist_ReturnsSlidesWithBase64Images()
        {
            // Arrange
            const int StreetcodeId = 1;
            const string BlobName = "blob1.jpg";
            const string Base64Value = "base64-image";

            var slides = PrepareSlidesWithImage(StreetcodeId, BlobName);
            var slideDtos = PrepareSlideDtosWithImage(StreetcodeId, BlobName, Base64Value);

            _repositoryWrapperMock
                .Setup(r => r.StreetcodeArtSlideRepository.GetAllAsync(
                    It.IsAny<Expression<Func<StreetcodeArtSlide, bool>>>(),
                    It.IsAny<Func<IQueryable<StreetcodeArtSlide>, IIncludableQueryable<StreetcodeArtSlide, object>>>()))
                .ReturnsAsync(slides);

            _mapperMock
                .Setup(m => m.Map<IEnumerable<StreetcodeArtSlideDTO>>(slides))
                .Returns(slideDtos);

            _blobServiceMock
                .Setup(b => b.FindFileInStorageAsBase64(BlobName))
                .Returns(Base64Value);

            var query = new GetArtSlidesByStreetcodeIdQuery(StreetcodeId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Single(result.Value);
            Assert.Equal(Base64Value, result.Value.First().StreetcodeArts.First().Art.Image.Base64);
            _blobServiceMock.Verify(b => b.FindFileInStorageAsBase64(BlobName), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenNoSlidesExist_ReturnsEmptyList()
        {
            // Arrange
            const int StreetcodeId = 1;
            _repositoryWrapperMock
                .Setup(r => r.StreetcodeArtSlideRepository.GetAllAsync(
                    It.IsAny<Expression<Func<StreetcodeArtSlide, bool>>>(),
                    It.IsAny<Func<IQueryable<StreetcodeArtSlide>, IIncludableQueryable<StreetcodeArtSlide, object>>>()))
                .ReturnsAsync(new List<StreetcodeArtSlide>());

            _mapperMock
                .Setup(m => m.Map<IEnumerable<StreetcodeArtSlideDTO>>(It.IsAny<IEnumerable<StreetcodeArtSlide>>()))
                .Returns(new List<StreetcodeArtSlideDTO>());

            var query = new GetArtSlidesByStreetcodeIdQuery(StreetcodeId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Empty(result.Value);
            _blobServiceMock.Verify(b => b.FindFileInStorageAsBase64(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenArtImageIsNull_SkipsBase64Loading()
        {
            // Arrange
            const int StreetcodeId = 1;
            var slides = PrepareSlidesWithImage(StreetcodeId, null!);
            var slideDtos = PrepareSlideDtosWithImage(StreetcodeId, null!, null!);

            _repositoryWrapperMock
                .Setup(r => r.StreetcodeArtSlideRepository.GetAllAsync(
                    It.IsAny<Expression<Func<StreetcodeArtSlide, bool>>>(),
                    It.IsAny<Func<IQueryable<StreetcodeArtSlide>, IIncludableQueryable<StreetcodeArtSlide, object>>>()))
                .ReturnsAsync(slides);

            _mapperMock
                .Setup(m => m.Map<IEnumerable<StreetcodeArtSlideDTO>>(slides))
                .Returns(slideDtos);

            var query = new GetArtSlidesByStreetcodeIdQuery(StreetcodeId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Single(result.Value);
            Assert.Null(result.Value.First().StreetcodeArts.First().Art.Image);
            _blobServiceMock.Verify(b => b.FindFileInStorageAsBase64(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenArtImageBlobNameIsNull_SkipsBase64Loading()
        {
            // Arrange
            const int StreetcodeId = 1;
            var slides = PrepareSlidesWithImage(StreetcodeId, null!);
            var slideDtos = PrepareSlideDtosWithImage(StreetcodeId, null!, null!);

            _repositoryWrapperMock
                .Setup(r => r.StreetcodeArtSlideRepository.GetAllAsync(
                    It.IsAny<Expression<Func<StreetcodeArtSlide, bool>>>(),
                    It.IsAny<Func<IQueryable<StreetcodeArtSlide>, IIncludableQueryable<StreetcodeArtSlide, object>>>()))
                .ReturnsAsync(slides);

            _mapperMock
                .Setup(m => m.Map<IEnumerable<StreetcodeArtSlideDTO>>(slides))
                .Returns(slideDtos);

            var query = new GetArtSlidesByStreetcodeIdQuery(StreetcodeId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Single(result.Value);
            Assert.Null(result.Value.First().StreetcodeArts.First().Art.Image);
            _blobServiceMock.Verify(b => b.FindFileInStorageAsBase64(It.IsAny<string>()), Times.Never);
        }

        private static List<StreetcodeArtSlide> PrepareSlidesWithImage(int streetcodeId, string blobName)
        {
            return new List<StreetcodeArtSlide>
            {
                new StreetcodeArtSlide
                {
                    Id = 1,
                    Index = 0,
                    Template = GallerySlideTemplate.OneToTwo,
                    StreetcodeId = streetcodeId,
                    StreetcodeArts = new List<StreetcodeArt>
                    {
                        new StreetcodeArt
                        {
                            Index = 0,
                            StreetcodeId = streetcodeId,
                            Art = new Art
                            {
                                Id = 1,
                                Image = blobName == null ? null : new Image
                                {
                                    Id = 1,
                                    BlobName = blobName
                                }
                            }
                        }
                    }
                }
            };
        }

        private static List<StreetcodeArtSlideDTO> PrepareSlideDtosWithImage(int streetcodeId, string blobName, string base64)
        {
            return new List<StreetcodeArtSlideDTO>
            {
                new StreetcodeArtSlideDTO
                {
                    Id = 1,
                    Index = 0,
                    Template = GallerySlideTemplate.OneToTwo,
                    StreetcodeId = streetcodeId,
                    StreetcodeArts = new List<StreetcodeArtDTO>
                    {
                        new StreetcodeArtDTO
                        {
                            Index = 0,
                            StreetcodeId = streetcodeId,
                            Art = new ArtDTO
                            {
                                Id = 1,
                                Image = blobName == null && base64 == null ? null : new ImageDTO
                                {
                                    Id = 1,
                                    BlobName = blobName,
                                    Base64 = base64
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}
