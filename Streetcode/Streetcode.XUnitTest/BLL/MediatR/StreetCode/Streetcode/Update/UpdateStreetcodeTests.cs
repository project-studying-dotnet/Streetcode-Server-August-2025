using System.Linq.Expressions;
using System.Transactions;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.ArtGallery;
using Streetcode.BLL.DTO.Media.Art;
using Streetcode.BLL.DTO.Streetcode.Update;
using Streetcode.BLL.Enums;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.Streetcode.Update;
using Streetcode.DAL.Entities.AdditionalContent;
using Streetcode.DAL.Entities.AdditionalContent.Coordinates.Types;
using Streetcode.DAL.Entities.Analytics;
using Streetcode.DAL.Entities.Media.Images;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Enums;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.StreetCode.Streetcode.Update
{
    public class UpdateStreetcodeTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILoggerService> _loggerServiceMock;
        private readonly UpdateStreetcodeHandler _handler;

        public UpdateStreetcodeTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _loggerServiceMock = new Mock<ILoggerService>();
            _handler = new UpdateStreetcodeHandler(_repositoryWrapperMock.Object, _mapperMock.Object, _loggerServiceMock.Object);
        }

        [Fact]
        public async Task Handle_Should_ReturnOk_WhenUpdateIsSuccessful()
        {
            // Arrange
            var entity = new StreetcodeContent { Id = 10, UpdatedAt = DateTime.MinValue };
            var requestDto = new StreetcodeUpdateDTO { Id = 10 };
            var request = new UpdateStreetcodeCommand(requestDto);

            _repositoryWrapperMock
                .Setup(r => r.StreetcodeRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<StreetcodeContent, bool>>>(),
                    It.IsAny<Func<IQueryable<StreetcodeContent>, IIncludableQueryable<StreetcodeContent, object>>>()))
                .ReturnsAsync(entity);

            _repositoryWrapperMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

            _mapperMock.Setup(m => m.Map(It.IsAny<StreetcodeUpdateDTO>(), It.IsAny<StreetcodeContent>()))
                .Verifiable();

            using var realTransactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            _repositoryWrapperMock.Setup(r => r.BeginTransaction()).Returns(realTransactionScope);

            SetupBasicMocks();

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess, $"Result should be successful but failed with: {string.Join(", ", result.Errors.Select(e => e.Message))}");
            Assert.Equal(10, result.Value);
            Assert.True(entity.UpdatedAt > DateTime.MinValue);

            _repositoryWrapperMock.Verify(r => r.SaveChangesAsync(), Times.AtLeast(1));
            _loggerServiceMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Once);
            _repositoryWrapperMock.Verify(r => r.BeginTransaction(), Times.Once);
            _mapperMock.Verify(m => m.Map(It.IsAny<StreetcodeUpdateDTO>(), It.IsAny<StreetcodeContent>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_ReturnFail_WhenEntityNotFound()
        {
            // Arrange
            var request = new UpdateStreetcodeCommand(new StreetcodeUpdateDTO { Id = 1 });

            _repositoryWrapperMock.Setup(r => r.StreetcodeRepository.GetFirstOrDefaultAsync(
               It.IsAny<Expression<Func<StreetcodeContent, bool>>>(),
               It.IsAny<Func<IQueryable<StreetcodeContent>, IIncludableQueryable<StreetcodeContent, object>>>()))
                .ReturnsAsync((StreetcodeContent)null!);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            _loggerServiceMock.Verify(l => l.LogError(request, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_WhenSaveChangesFails()
        {
            // Arrange
            var entity = new StreetcodeContent { Id = 1 };
            var request = new UpdateStreetcodeCommand(new StreetcodeUpdateDTO { Id = 1 });

            _repositoryWrapperMock.Setup(r => r.StreetcodeRepository.GetFirstOrDefaultAsync(
               It.IsAny<Expression<Func<StreetcodeContent, bool>>>(),
               It.IsAny<Func<IQueryable<StreetcodeContent>, IIncludableQueryable<StreetcodeContent, object>>>()))
                .ReturnsAsync(entity);

            _repositoryWrapperMock.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(-1);

            SetupBasicMocks();

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            _loggerServiceMock.Verify(l => l.LogError(request, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_WhenThrowException()
        {
            // Arrange
            var request = new UpdateStreetcodeCommand(new StreetcodeUpdateDTO { Id = 1 });

            _repositoryWrapperMock.Setup(r => r.StreetcodeRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<StreetcodeContent, bool>>>(),
                It.IsAny<Func<IQueryable<StreetcodeContent>, IIncludableQueryable<StreetcodeContent, object>>>()))
                .ThrowsAsync(new Exception("DB error"));

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains("DB error", result.Errors[0].Message);
            _loggerServiceMock.Verify(l => l.LogError(request, "DB error"), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_UpdateArtGallery_WhenValidArtsAndSlidesProvided()
        {
            // Arrange
            var entity = new StreetcodeContent { Id = 1 };
            var requestDto = CreateRequestWithArtGallery();
            var request = new UpdateStreetcodeCommand(requestDto);

            SetupSuccessfulUpdate(entity);
            SetupArtGalleryMocks();

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            VerifyArtGalleryUpdateCalls();
        }

        [Fact]
        public async Task Handle_Should_SkipArtGalleryUpdate_WhenNoArtsOrSlidesProvided()
        {
            // Arrange
            var entity = new StreetcodeContent { Id = 1 };
            var requestDto = new StreetcodeUpdateDTO { Id = 1 };
            var request = new UpdateStreetcodeCommand(requestDto);

            SetupSuccessfulUpdate(entity);
            SetupBasicMocks();

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            VerifyArtRepositoriesNotCalled();
        }

        [Fact]
        public async Task Handle_Should_CreateNewArts_WhenArtsWithCreatedModelState()
        {
            // Arrange
            var entity = new StreetcodeContent { Id = 1 };
            var requestDto = CreateRequestWithNewArts();
            var request = new UpdateStreetcodeCommand(requestDto);

            SetupSuccessfulUpdate(entity);
            SetupArtGalleryMocks();

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            _repositoryWrapperMock.Verify(r => r.ArtRepository.CreateRangeAsync(It.IsAny<IEnumerable<Art>>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_UpdateExistingArts_WhenArtsWithUpdatedModelState()
        {
            // Arrange
            var entity = new StreetcodeContent { Id = 1 };
            var requestDto = CreateRequestWithUpdatedArts();
            var request = new UpdateStreetcodeCommand(requestDto);

            SetupSuccessfulUpdate(entity);
            SetupArtGalleryMocks();

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            _repositoryWrapperMock.Verify(r => r.ArtRepository.UpdateRange(It.IsAny<IEnumerable<Art>>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_DeleteArts_WhenArtsWithDeletedModelState()
        {
            // Arrange
            var entity = new StreetcodeContent { Id = 1 };
            var requestDto = CreateRequestWithDeletedArts();
            var request = new UpdateStreetcodeCommand(requestDto);

            SetupSuccessfulUpdate(entity);
            SetupArtGalleryMocks();

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            _repositoryWrapperMock.Verify(r => r.ArtRepository.DeleteRange(It.IsAny<IEnumerable<Art>>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_CreateNewSlides_WhenSlidesWithCreatedModelState()
        {
            // Arrange
            var entity = new StreetcodeContent { Id = 1 };
            var requestDto = CreateRequestWithNewSlides();
            var request = new UpdateStreetcodeCommand(requestDto);

            SetupSuccessfulUpdate(entity);
            SetupArtGalleryMocks();

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            _repositoryWrapperMock.Verify(r => r.StreetcodeArtSlideRepository.CreateRangeAsync(It.IsAny<IEnumerable<StreetcodeArtSlide>>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_UpdateExistingSlides_WhenSlidesWithUpdatedModelState()
        {
            // Arrange
            var entity = new StreetcodeContent { Id = 1 };
            var requestDto = CreateRequestWithUpdatedSlides();
            var request = new UpdateStreetcodeCommand(requestDto);

            SetupSuccessfulUpdate(entity);
            SetupArtGalleryMocks();

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            _repositoryWrapperMock.Verify(r => r.StreetcodeArtSlideRepository.UpdateRange(It.IsAny<IEnumerable<StreetcodeArtSlide>>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_DeleteSlides_WhenSlidesWithDeletedModelState()
        {
            // Arrange
            var entity = new StreetcodeContent { Id = 1 };
            var requestDto = CreateRequestWithDeletedSlides();
            var request = new UpdateStreetcodeCommand(requestDto);

            SetupSuccessfulUpdate(entity);
            SetupArtGalleryMocks();

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            _repositoryWrapperMock.Verify(r => r.StreetcodeArtSlideRepository.DeleteRange(It.IsAny<IEnumerable<StreetcodeArtSlide>>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_RebuildStreetcodeArts_WhenSlidesProvided()
        {
            // Arrange
            var entity = new StreetcodeContent { Id = 1 };
            var requestDto = CreateRequestWithArtGallery();
            var request = new UpdateStreetcodeCommand(requestDto);

            SetupSuccessfulUpdate(entity);
            SetupArtGalleryMocks();

            // Mock existing StreetcodeArts
            var existingStreetcodeArts = new List<StreetcodeArt>
            {
                new StreetcodeArt { Id = 1, StreetcodeId = 1, ArtId = 1, StreetcodeArtSlideId = 1 }
            };
            _repositoryWrapperMock.Setup(r => r.StreetcodeArtRepository.GetAllAsync(
                It.IsAny<Expression<Func<StreetcodeArt, bool>>>(),
                It.IsAny<Func<IQueryable<StreetcodeArt>, IIncludableQueryable<StreetcodeArt, object>>>()))
                .ReturnsAsync(existingStreetcodeArts);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            _repositoryWrapperMock.Verify(r => r.StreetcodeArtRepository.DeleteRange(existingStreetcodeArts), Times.Once);
            _repositoryWrapperMock.Verify(r => r.StreetcodeArtRepository.CreateRangeAsync(It.IsAny<IEnumerable<StreetcodeArt>>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_ThrowException_WhenImageDoesNotExistForArt()
        {
            // Arrange
            var entity = new StreetcodeContent { Id = 1 };
            var requestDto = CreateRequestWithInvalidImageId();
            var request = new UpdateStreetcodeCommand(requestDto);

            SetupSuccessfulUpdate(entity);
            SetupBasicMocks();

            // Setup empty image list (image doesn't exist)
            _repositoryWrapperMock.Setup(r => r.ImageRepository.GetAllAsync(
                It.IsAny<Expression<Func<Image, bool>>>(),
                It.IsAny<Func<IQueryable<Image>, IIncludableQueryable<Image, object>>>()))
                .ReturnsAsync(new List<Image>());

            // Act & Assert
            var result = await _handler.Handle(request, CancellationToken.None);
            Assert.True(result.IsFailed);
            Assert.Contains("Image with ID 999 does not exist", result.Errors[0].Message);
        }

        [Fact]
        public async Task Handle_Should_ThrowException_WhenArtDoesNotExistForStreetcodeArt()
        {
            // Arrange
            var entity = new StreetcodeContent { Id = 1 };
            var requestDto = CreateRequestWithInvalidArtId();
            var request = new UpdateStreetcodeCommand(requestDto);

            SetupSuccessfulUpdate(entity);
            SetupArtGalleryMocks();

            // Setup art repository to return null for non-existent art
            _repositoryWrapperMock.Setup(r => r.ArtRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<Art, bool>>>(),
                It.IsAny<Func<IQueryable<Art>, IIncludableQueryable<Art, object>>>()))
                .ReturnsAsync((Art)null!);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains("Art with ID 999 does not exist", result.Errors[0].Message);
        }

        private void SetupBasicMocks()
        {
            // Setup basic repository mocks for tags and images
            _repositoryWrapperMock.Setup(r => r.StreetcodeTagIndexRepository.CreateRangeAsync(It.IsAny<IEnumerable<StreetcodeTagIndex>>()))
                .Returns(Task.CompletedTask);
            _repositoryWrapperMock.Setup(r => r.StreetcodeTagIndexRepository.DeleteRange(It.IsAny<IEnumerable<StreetcodeTagIndex>>()))
                .Verifiable();
            _repositoryWrapperMock.Setup(r => r.StreetcodeTagIndexRepository.UpdateRange(It.IsAny<IEnumerable<StreetcodeTagIndex>>()))
                .Verifiable();

            _repositoryWrapperMock.Setup(r => r.StreetcodeCoordinateRepository.CreateRangeAsync(It.IsAny<IEnumerable<StreetcodeCoordinate>>()))
                .Returns(Task.CompletedTask);
            _repositoryWrapperMock.Setup(r => r.StreetcodeCoordinateRepository.DeleteRange(It.IsAny<IEnumerable<StreetcodeCoordinate>>()))
                .Verifiable();
            _repositoryWrapperMock.Setup(r => r.StreetcodeCoordinateRepository.UpdateRange(It.IsAny<IEnumerable<StreetcodeCoordinate>>()))
                .Verifiable();

            _repositoryWrapperMock.Setup(r => r.ImageRepository.DeleteRange(It.IsAny<IEnumerable<Image>>()))
                .Verifiable();
            _repositoryWrapperMock.Setup(r => r.StreetcodeImageRepository.CreateRangeAsync(It.IsAny<IEnumerable<StreetcodeImage>>()))
                .Returns(Task.CompletedTask);

            // Setup transaction
            _repositoryWrapperMock.Setup(r => r.BeginTransaction()).Returns(new TransactionScope(TransactionScopeOption.Suppress));
        }

        private void SetupArtGalleryMocks()
        {
            SetupBasicMocks();

            // Setup Art repository mocks
            _repositoryWrapperMock.Setup(r => r.ArtRepository.CreateRangeAsync(It.IsAny<IEnumerable<Art>>()))
                .Returns(Task.CompletedTask);
            _repositoryWrapperMock.Setup(r => r.ArtRepository.UpdateRange(It.IsAny<IEnumerable<Art>>()))
                .Verifiable();
            _repositoryWrapperMock.Setup(r => r.ArtRepository.DeleteRange(It.IsAny<IEnumerable<Art>>()))
                .Verifiable();

            // Setup StreetcodeArtSlide repository mocks
            _repositoryWrapperMock.Setup(r => r.StreetcodeArtSlideRepository.CreateRangeAsync(It.IsAny<IEnumerable<StreetcodeArtSlide>>()))
                .Returns(Task.CompletedTask);
            _repositoryWrapperMock.Setup(r => r.StreetcodeArtSlideRepository.UpdateRange(It.IsAny<IEnumerable<StreetcodeArtSlide>>()))
                .Verifiable();
            _repositoryWrapperMock.Setup(r => r.StreetcodeArtSlideRepository.DeleteRange(It.IsAny<IEnumerable<StreetcodeArtSlide>>()))
                .Verifiable();

            // Setup StreetcodeArt repository mocks
            _repositoryWrapperMock.Setup(r => r.StreetcodeArtRepository.CreateRangeAsync(It.IsAny<IEnumerable<StreetcodeArt>>()))
                .Returns(Task.CompletedTask);
            _repositoryWrapperMock.Setup(r => r.StreetcodeArtRepository.DeleteRange(It.IsAny<IEnumerable<StreetcodeArt>>()))
                .Verifiable();
            _repositoryWrapperMock.Setup(r => r.StreetcodeArtRepository.GetAllAsync(
                It.IsAny<Expression<Func<StreetcodeArt, bool>>>(),
                It.IsAny<Func<IQueryable<StreetcodeArt>, IIncludableQueryable<StreetcodeArt, object>>>()))
                .ReturnsAsync(new List<StreetcodeArt>());

            // Setup Image repository for validation
            _repositoryWrapperMock.Setup(r => r.ImageRepository.GetAllAsync(
                It.IsAny<Expression<Func<Image, bool>>>(),
                It.IsAny<Func<IQueryable<Image>, IIncludableQueryable<Image, object>>>()))
                .ReturnsAsync(new List<Image> { new Image { Id = 1 }, new Image { Id = 2 } });

            // Setup Art repository for validation
            _repositoryWrapperMock.Setup(r => r.ArtRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<Art, bool>>>(),
                It.IsAny<Func<IQueryable<Art>, IIncludableQueryable<Art, object>>>()))
                .ReturnsAsync(new Art { Id = 1 });

            // Setup created slide lookup
            _repositoryWrapperMock.Setup(r => r.StreetcodeArtSlideRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<StreetcodeArtSlide, bool>>>(),
                It.IsAny<Func<IQueryable<StreetcodeArtSlide>, IIncludableQueryable<StreetcodeArtSlide, object>>>()))
                .ReturnsAsync(new StreetcodeArtSlide { Id = 1, StreetcodeId = 1, Index = 0, Template = GallerySlideTemplate.OneToTwo });

            // Setup mapper for art gallery entities
            _mapperMock.Setup(m => m.Map<IEnumerable<Art>>(It.IsAny<IEnumerable<ArtCreateUpdateDTO>>()))
                .Returns(new List<Art> { new Art { Id = 1, Title = "Test Art" } });
            _mapperMock.Setup(m => m.Map<IEnumerable<StreetcodeArtSlide>>(It.IsAny<IEnumerable<StreetcodeArtSlideCreateUpdateDTO>>()))
                .Returns(new List<StreetcodeArtSlide> { new StreetcodeArtSlide { Id = 1, Index = 0, Template = GallerySlideTemplate.OneToTwo } });
        }

        private void SetupSuccessfulUpdate(StreetcodeContent entity)
        {
            _repositoryWrapperMock.Setup(r => r.StreetcodeRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<StreetcodeContent, bool>>>(),
                It.IsAny<Func<IQueryable<StreetcodeContent>, IIncludableQueryable<StreetcodeContent, object>>>()))
                .ReturnsAsync(entity);

            _repositoryWrapperMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

            _mapperMock.Setup(m => m.Map(It.IsAny<StreetcodeUpdateDTO>(), It.IsAny<StreetcodeContent>()))
                .Verifiable();
        }

        private StreetcodeUpdateDTO CreateRequestWithArtGallery()
        {
            return new StreetcodeUpdateDTO
            {
                Id = 1,
                Arts = new List<ArtCreateUpdateDTO>
                {
                    new ArtCreateUpdateDTO { Id = 1, Title = "Art 1", Description = "Desc 1", ImageId = 1, ModelState = ModelState.Created }
                },
                StreetcodeArtSlides = new List<StreetcodeArtSlideCreateUpdateDTO>
                {
                    new StreetcodeArtSlideCreateUpdateDTO
                    {
                        Id = 1,
                        Index = 0,
                        Template = GallerySlideTemplate.OneToTwo,
                        ModelState = ModelState.Created,
                        StreetcodeArts = new List<StreetcodeArtCreateUpdateDTO>
                        {
                            new StreetcodeArtCreateUpdateDTO { Index = 0, ArtId = 1 }
                        }
                    }
                }
            };
        }

        private StreetcodeUpdateDTO CreateRequestWithNewArts()
        {
            return new StreetcodeUpdateDTO
            {
                Id = 1,
                Arts = new List<ArtCreateUpdateDTO>
                {
                    new ArtCreateUpdateDTO { Id = 1, Title = "New Art", Description = "New Desc", ImageId = 1, ModelState = ModelState.Created }
                }
            };
        }

        private StreetcodeUpdateDTO CreateRequestWithUpdatedArts()
        {
            return new StreetcodeUpdateDTO
            {
                Id = 1,
                Arts = new List<ArtCreateUpdateDTO>
                {
                    new ArtCreateUpdateDTO { Id = 1, Title = "Updated Art", Description = "Updated Desc", ImageId = 1, ModelState = ModelState.Updated }
                }
            };
        }

        private StreetcodeUpdateDTO CreateRequestWithDeletedArts()
        {
            return new StreetcodeUpdateDTO
            {
                Id = 1,
                Arts = new List<ArtCreateUpdateDTO>
                {
                    new ArtCreateUpdateDTO { Id = 1, Title = "Art to Delete", Description = "Delete Desc", ImageId = 1, ModelState = ModelState.Deleted }
                }
            };
        }

        private StreetcodeUpdateDTO CreateRequestWithNewSlides()
        {
            return new StreetcodeUpdateDTO
            {
                Id = 1,
                StreetcodeArtSlides = new List<StreetcodeArtSlideCreateUpdateDTO>
                {
                    new StreetcodeArtSlideCreateUpdateDTO
                    {
                        Id = 1,
                        Index = 0,
                        Template = GallerySlideTemplate.OneToTwo,
                        ModelState = ModelState.Created,
                        StreetcodeArts = new List<StreetcodeArtCreateUpdateDTO>()
                    }
                }
            };
        }

        private StreetcodeUpdateDTO CreateRequestWithUpdatedSlides()
        {
            return new StreetcodeUpdateDTO
            {
                Id = 1,
                StreetcodeArtSlides = new List<StreetcodeArtSlideCreateUpdateDTO>
                {
                    new StreetcodeArtSlideCreateUpdateDTO
                    {
                        Id = 1,
                        Index = 0,
                        Template = GallerySlideTemplate.OneAndTwo,
                        ModelState = ModelState.Updated,
                        StreetcodeArts = new List<StreetcodeArtCreateUpdateDTO>()
                    }
                }
            };
        }

        private StreetcodeUpdateDTO CreateRequestWithDeletedSlides()
        {
            return new StreetcodeUpdateDTO
            {
                Id = 1,
                StreetcodeArtSlides = new List<StreetcodeArtSlideCreateUpdateDTO>
                {
                    new StreetcodeArtSlideCreateUpdateDTO
                    {
                        Id = 1,
                        Index = 0,
                        Template = GallerySlideTemplate.OneToTwo,
                        ModelState = ModelState.Deleted,
                        StreetcodeArts = new List<StreetcodeArtCreateUpdateDTO>()
                    }
                }
            };
        }

        private StreetcodeUpdateDTO CreateRequestWithInvalidImageId()
        {
            return new StreetcodeUpdateDTO
            {
                Id = 1,
                Arts = new List<ArtCreateUpdateDTO>
                {
                    new ArtCreateUpdateDTO { Id = 1, Title = "Art with invalid image", Description = "Desc", ImageId = 999, ModelState = ModelState.Created }
                }
            };
        }

        private StreetcodeUpdateDTO CreateRequestWithInvalidArtId()
        {
            return new StreetcodeUpdateDTO
            {
                Id = 1,
                StreetcodeArtSlides = new List<StreetcodeArtSlideCreateUpdateDTO>
                {
                    new StreetcodeArtSlideCreateUpdateDTO
                    {
                        Id = 1,
                        Index = 0,
                        Template = GallerySlideTemplate.OneToTwo,
                        ModelState = ModelState.Created,
                        StreetcodeArts = new List<StreetcodeArtCreateUpdateDTO>
                        {
                            new StreetcodeArtCreateUpdateDTO { Index = 0, ArtId = 999 }
                        }
                    }
                }
            };
        }

        private void VerifyArtGalleryUpdateCalls()
        {
            _repositoryWrapperMock.Verify(r => r.ArtRepository.CreateRangeAsync(It.IsAny<IEnumerable<Art>>()), Times.Once);
            _repositoryWrapperMock.Verify(r => r.StreetcodeArtSlideRepository.CreateRangeAsync(It.IsAny<IEnumerable<StreetcodeArtSlide>>()), Times.Once);
            _repositoryWrapperMock.Verify(r => r.StreetcodeArtRepository.CreateRangeAsync(It.IsAny<IEnumerable<StreetcodeArt>>()), Times.Once);
        }

        private void VerifyArtRepositoriesNotCalled()
        {
            _repositoryWrapperMock.Verify(r => r.ArtRepository.CreateRangeAsync(It.IsAny<IEnumerable<Art>>()), Times.Never);
            _repositoryWrapperMock.Verify(r => r.ArtRepository.UpdateRange(It.IsAny<IEnumerable<Art>>()), Times.Never);
            _repositoryWrapperMock.Verify(r => r.ArtRepository.DeleteRange(It.IsAny<IEnumerable<Art>>()), Times.Never);
            _repositoryWrapperMock.Verify(r => r.StreetcodeArtSlideRepository.CreateRangeAsync(It.IsAny<IEnumerable<StreetcodeArtSlide>>()), Times.Never);
            _repositoryWrapperMock.Verify(r => r.StreetcodeArtSlideRepository.UpdateRange(It.IsAny<IEnumerable<StreetcodeArtSlide>>()), Times.Never);
            _repositoryWrapperMock.Verify(r => r.StreetcodeArtSlideRepository.DeleteRange(It.IsAny<IEnumerable<StreetcodeArtSlide>>()), Times.Never);
            _repositoryWrapperMock.Verify(r => r.StreetcodeArtRepository.CreateRangeAsync(It.IsAny<IEnumerable<StreetcodeArt>>()), Times.Never);
        }
    }
}
