using System.Transactions;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.AdditionalContent.Tag;
using Streetcode.BLL.DTO.ArtGallery;
using Streetcode.BLL.DTO.Media.Art;
using Streetcode.BLL.DTO.Media.Images;
using Streetcode.BLL.DTO.Streetcode;
using Streetcode.BLL.DTO.Streetcode.Create;
using Streetcode.BLL.Enums;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.Streetcode.Create;
using Streetcode.DAL.Entities.AdditionalContent;
using Streetcode.DAL.Entities.Media.Images;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Enums;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.StreetCode.StreetCode.Create;

public class StreetcodeCreateHandlerTest
{
    private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly StreetcodeCreateHandler _handler;

    public StreetcodeCreateHandlerTest()
    {
        _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILoggerService>();
        _handler = new StreetcodeCreateHandler(
            _mockRepositoryWrapper.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var request = CreateValidRequest();
        var streetcodeEntity = CreateStreetcodeEntity();
        var streetcodeDto = CreateStreetcodeDTO();

        SetupMocksForCompleteSuccess(request, streetcodeEntity, streetcodeDto);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(streetcodeDto, result.Value);
        VerifySuccessCalls(request, streetcodeEntity);
    }

    [Fact]
    public async Task Handle_SaveStreetcodeFails_ShouldReturnFailure()
    {
        // Arrange
        var request = CreateValidRequest();
        var streetcodeEntity = CreateStreetcodeEntity();

        _mockMapper.Setup(m => m.Map<StreetcodeContent>(request.NewStreetcode))
            .Returns(streetcodeEntity);
        _mockRepositoryWrapper.Setup(r => r.StreetcodeRepository.Create(streetcodeEntity));
        _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(0);
        _mockRepositoryWrapper.Setup(r => r.BeginTransaction())
            .Returns(new TransactionScope());

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Contains("Failed to save streetcode to database", result.Errors[0].Message);
        _mockLogger.Verify(l => l.LogError(request, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Handle_EmptyImagesDetails_ShouldReturnFailure()
    {
        // Arrange
        var request = CreateRequestWithEmptyImages();
        var streetcodeEntity = CreateStreetcodeEntity();

        SetupMocksForInitialSave(request, streetcodeEntity);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Contains("ImagesDetails cannot be empty", result.Errors[0].Message);
    }

    [Fact]
    public async Task Handle_EmptyImageIds_ShouldReturnFailure()
    {
        // Arrange
        var request = CreateRequestWithInvalidImageIds();
        var streetcodeEntity = CreateStreetcodeEntity();

        SetupMocksForInitialSave(request, streetcodeEntity);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Contains("Image IDs cannot be empty", result.Errors[0].Message);
    }

    [Fact]
    public async Task Handle_EmptyTags_ShouldReturnFailure()
    {
        // Arrange
        var request = CreateRequestWithEmptyTags();
        var streetcodeEntity = CreateStreetcodeEntity();

        SetupMocksForInitialSave(request, streetcodeEntity);
        SetupImagesForSuccess();

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Contains("Tags cannot be empty", result.Errors[0].Message);
    }

    [Fact]
    public async Task Handle_ExceptionThrown_ShouldReturnFailure()
    {
        // Arrange
        var request = CreateValidRequest();

        _mockMapper.Setup(m => m.Map<StreetcodeContent>(request.NewStreetcode))
            .Throws(new Exception("Test exception"));
        _mockRepositoryWrapper.Setup(r => r.BeginTransaction())
            .Returns(new TransactionScope());

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Contains("Test exception", result.Errors[0].Message);
        _mockLogger.Verify(l => l.LogError(request, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidArtsAndSlides_ShouldCreateArtGallerySuccessfully()
    {
        // Arrange
        var request = CreateRequestWithArtGallery();
        var streetcodeEntity = CreateStreetcodeEntity();
        var streetcodeDto = CreateStreetcodeDTO();

        SetupMocksForArtGallerySuccess(request, streetcodeEntity, streetcodeDto);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        VerifyArtGalleryCreationCalls();
    }

    [Fact]
    public async Task Handle_WithEmptyArtGallery_ShouldSkipArtGalleryCreation()
    {
        // Arrange
        var request = CreateValidRequest(); // No arts or slides
        var streetcodeEntity = CreateStreetcodeEntity();
        var streetcodeDto = CreateStreetcodeDTO();

        SetupMocksForCompleteSuccess(request, streetcodeEntity, streetcodeDto);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        // Verify art repositories were not called
        _mockRepositoryWrapper.Verify(r => r.ArtRepository.CreateRangeAsync(It.IsAny<IEnumerable<Art>>()), Times.Never);
        _mockRepositoryWrapper.Verify(r => r.StreetcodeArtSlideRepository.CreateRangeAsync(It.IsAny<IEnumerable<StreetcodeArtSlide>>()), Times.Never);
        _mockRepositoryWrapper.Verify(r => r.StreetcodeArtRepository.CreateRangeAsync(It.IsAny<IEnumerable<StreetcodeArt>>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNonExistentImageId_ShouldThrowException()
    {
        // Arrange
        var request = CreateRequestWithInvalidImageIdInArt();
        var streetcodeEntity = CreateStreetcodeEntity();

        SetupMocksForInitialSave(request, streetcodeEntity);
        SetupImagesForSuccess();
        SetupTagsForSuccess();
        SetupImagesDetailsForSuccess();

        // Setup image repository to return null for non-existent image
        _mockRepositoryWrapper.Setup(r => r.ImageRepository.GetAllAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Image, bool>>>(),
                It.IsAny<Func<IQueryable<Image>, IIncludableQueryable<Image, object>>>()))
            .ReturnsAsync([]);

        _mockRepositoryWrapper.Setup(r => r.BeginTransaction())
            .Returns(new TransactionScope());

        // Act & Assert
        var result = await _handler.Handle(request, CancellationToken.None);
        Assert.True(result.IsFailed);
        Assert.Contains("Image with ID 999 does not exist", result.Errors[0].Message);
    }

    [Fact]
    public async Task Handle_WithNonExistentArtId_ShouldThrowException()
    {
        // Arrange
        var request = CreateRequestWithInvalidArtIdInSlide();
        var streetcodeEntity = CreateStreetcodeEntity();

        SetupMocksForInitialSave(request, streetcodeEntity);
        SetupImagesForSuccess();
        SetupTagsForSuccess();
        SetupImagesDetailsForSuccess();

        // Setup art repository to return null for non-existent art
        _mockRepositoryWrapper.Setup(r => r.ImageRepository.GetAllAsync(
        It.IsAny<System.Linq.Expressions.Expression<Func<Image, bool>>>(),
        It.IsAny<Func<IQueryable<Image>, IIncludableQueryable<Image, object>>>()))
            .ReturnsAsync([]);
        _mockMapper.Setup(m => m.Map<List<Art>>(It.IsAny<List<ArtCreateUpdateDTO>>()))
            .Returns([]);

        _mockMapper.Setup(m => m.Map<List<StreetcodeArtSlide>>(It.IsAny<List<StreetcodeArtSlideCreateUpdateDTO>>()))
            .Returns([new StreetcodeArtSlide { Id = 1, Index = 0, StreetcodeId = 1 }]);

        _mockRepositoryWrapper.Setup(r => r.StreetcodeArtSlideRepository.CreateRangeAsync(It.IsAny<IEnumerable<StreetcodeArtSlide>>()))
            .Callback<IEnumerable<StreetcodeArtSlide>>(slides =>
            {
                // Simulate setting IDs for created entities
                var slideList = slides.ToList();
                for (int i = 0; i < slideList.Count; i++)
                {
                    slideList[i].Id = i + 1;
                }
            })
            .Returns(Task.CompletedTask);

        _mockRepositoryWrapper.Setup(r => r.ArtRepository.GetFirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Art, bool>>>(),
                It.IsAny<Func<IQueryable<Art>, IIncludableQueryable<Art, object>>>()))
            .ReturnsAsync((Art?)null);

        _mockRepositoryWrapper.Setup(r => r.BeginTransaction())
            .Returns(new TransactionScope());

        // Act & Assert
        var result = await _handler.Handle(request, CancellationToken.None);
        Assert.True(result.IsFailed);
        Assert.Contains("Art ID '999' not found in the mapped arts.", result.Errors[0].Message);
    }

    [Fact]
    public async Task Handle_WithFilteredArts_ShouldOnlyCreateUsedArts()
    {
        // Arrange
        var request = CreateRequestWithUnusedArts();
        var streetcodeEntity = CreateStreetcodeEntity();
        var streetcodeDto = CreateStreetcodeDTO();

        SetupMocksForFilteredArtsSuccess(request, streetcodeEntity, streetcodeDto);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        // Verify that only arts referenced in slides were created
        _mockRepositoryWrapper.Verify(
            r => r.ArtRepository.CreateRangeAsync(
                It.Is<IEnumerable<Art>>(arts => arts.Count() == 2)),
            Times.Once); // Only 2 out of 3 arts should be created
    }

    [Fact]
    public async Task Handle_WithMultipleSlides_ShouldCreateAllSlidesAndArts()
    {
        // Arrange
        var request = CreateRequestWithMultipleSlides();
        var streetcodeEntity = CreateStreetcodeEntity();
        var streetcodeDto = CreateStreetcodeDTO();

        SetupMocksForMultipleSlidesSuccess(request, streetcodeEntity, streetcodeDto);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        // Verify multiple slides and arts were created
        _mockRepositoryWrapper.Verify(
            r => r.StreetcodeArtSlideRepository.CreateRangeAsync(
                It.Is<IEnumerable<StreetcodeArtSlide>>(slides => slides.Count() == 2)),
            Times.Once);
        _mockRepositoryWrapper.Verify(
            r => r.StreetcodeArtRepository.CreateRangeAsync(
                It.Is<IEnumerable<StreetcodeArt>>(streetcodeArts => streetcodeArts.Count() == 3)),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithArtGallery_ShouldSetCorrectRelationships()
    {
        // Arrange
        var request = CreateRequestWithArtGallery();
        var streetcodeEntity = CreateStreetcodeEntity();
        var streetcodeDto = CreateStreetcodeDTO();

        var capturedArtSlides = new List<StreetcodeArtSlide>();
        var capturedStreetcodeArts = new List<StreetcodeArt>();

        SetupMocksForArtGallerySuccess(request, streetcodeEntity, streetcodeDto);

        _mockRepositoryWrapper.Setup(r => r.StreetcodeArtSlideRepository.CreateRangeAsync(It.IsAny<IEnumerable<StreetcodeArtSlide>>()))
            .Callback<IEnumerable<StreetcodeArtSlide>>(capturedArtSlides.AddRange)
            .Returns(Task.CompletedTask);

        _mockRepositoryWrapper.Setup(r => r.StreetcodeArtRepository.CreateRangeAsync(It.IsAny<IEnumerable<StreetcodeArt>>()))
            .Callback<IEnumerable<StreetcodeArt>>(capturedStreetcodeArts.AddRange)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        // Verify relationships are set correctly
        Assert.All(capturedArtSlides, slide => Assert.Equal(streetcodeEntity.Id, slide.StreetcodeId));
        Assert.All(
            capturedStreetcodeArts,
            streetcodeArt =>
            {
                Assert.Equal(streetcodeEntity.Id, streetcodeArt.StreetcodeId);
                Assert.True(streetcodeArt.StreetcodeArtSlideId > 0);
                Assert.True(streetcodeArt.ArtId > 0);
            });
    }

    private StreetcodeCreateCommand CreateValidRequest()
    {
        return new StreetcodeCreateCommand(new StreetcodeCreateDTO
        {
            Title = "Test Streetcode",
            TransliterationUrl = "test-streetcode",
            Index = 1,
            Teaser = "Test teaser",
            DateString = "2024",
            StreetcodeType = StreetcodeType.Person,
            Status = StreetcodeStatus.Published,
            ImagesDetails = new List<ImageDetailsDto>
            {
                new ImageDetailsDto { ImageId = 1, Alt = "1" }
            },
            Tags = new List<StreetcodeTagDTO>
            {
                new StreetcodeTagDTO { Id = 1, Title = "Test Tag", IsVisible = true }
            }
        });
    }

    private StreetcodeCreateCommand CreateRequestWithArtGallery()
    {
        var request = CreateValidRequest();
        request.NewStreetcode.Arts = new List<ArtCreateUpdateDTO>
        {
            new ArtCreateUpdateDTO { Id = 1, Title = "Art 1", Description = "Desc 1", ImageId = 1, ModelState = ModelState.Created },
            new ArtCreateUpdateDTO { Id = 2, Title = "Art 2", Description = "Desc 2", ImageId = 2, ModelState = ModelState.Created }
        };
        request.NewStreetcode.StreetcodeArtSlides = new List<StreetcodeArtSlideCreateUpdateDTO>
        {
            new StreetcodeArtSlideCreateUpdateDTO
            {
                Index = 0,
                Template = GallerySlideTemplate.OneToTwo,
                StreetcodeArts = new List<StreetcodeArtCreateUpdateDTO>
                {
                    new StreetcodeArtCreateUpdateDTO { Index = 0, ArtId = 1 },
                    new StreetcodeArtCreateUpdateDTO { Index = 1, ArtId = 2 }
                }
            }
        };
        return request;
    }

    private StreetcodeCreateCommand CreateRequestWithUnusedArts()
    {
        var request = CreateValidRequest();
        request.NewStreetcode.Arts = new List<ArtCreateUpdateDTO>
        {
            new ArtCreateUpdateDTO { Id = 1, Title = "Art 1", Description = "Desc 1", ImageId = 1, ModelState = ModelState.Created },
            new ArtCreateUpdateDTO { Id = 2, Title = "Art 2", Description = "Desc 2", ImageId = 2, ModelState = ModelState.Created },
            new ArtCreateUpdateDTO { Id = 3, Title = "Art 3", Description = "Desc 3", ImageId = 3, ModelState = ModelState.Created } // Not used in slides
        };
        request.NewStreetcode.StreetcodeArtSlides = new List<StreetcodeArtSlideCreateUpdateDTO>
        {
            new StreetcodeArtSlideCreateUpdateDTO
            {
                Index = 0,
                Template = GallerySlideTemplate.OneToTwo,
                StreetcodeArts = new List<StreetcodeArtCreateUpdateDTO>
                {
                    new StreetcodeArtCreateUpdateDTO { Index = 0, ArtId = 1 },
                    new StreetcodeArtCreateUpdateDTO { Index = 1, ArtId = 2 }
                }
            }
        };
        return request;
    }

    private StreetcodeCreateCommand CreateRequestWithMultipleSlides()
    {
        var request = CreateValidRequest();
        request.NewStreetcode.Arts = new List<ArtCreateUpdateDTO>
        {
            new ArtCreateUpdateDTO { Id = 1, Title = "Art 1", Description = "Desc 1", ImageId = 1, ModelState = ModelState.Created },
            new ArtCreateUpdateDTO { Id = 2, Title = "Art 2", Description = "Desc 2", ImageId = 2, ModelState = ModelState.Created },
            new ArtCreateUpdateDTO { Id = 3, Title = "Art 3", Description = "Desc 3", ImageId = 3, ModelState = ModelState.Created }
        };
        request.NewStreetcode.StreetcodeArtSlides = new List<StreetcodeArtSlideCreateUpdateDTO>
        {
            new StreetcodeArtSlideCreateUpdateDTO
            {
                Index = 0,
                Template = GallerySlideTemplate.OneToTwo,
                StreetcodeArts = new List<StreetcodeArtCreateUpdateDTO>
                {
                    new StreetcodeArtCreateUpdateDTO { Index = 0, ArtId = 1 },
                    new StreetcodeArtCreateUpdateDTO { Index = 1, ArtId = 2 }
                }
            },
            new StreetcodeArtSlideCreateUpdateDTO
            {
                Index = 1,
                Template = GallerySlideTemplate.OneAndTwo,
                StreetcodeArts = new List<StreetcodeArtCreateUpdateDTO>
                {
                    new StreetcodeArtCreateUpdateDTO { Index = 0, ArtId = 3 }
                }
            }
        };
        return request;
    }

    private StreetcodeCreateCommand CreateRequestWithInvalidImageIdInArt()
    {
        var request = CreateValidRequest();
        request.NewStreetcode.Arts = new List<ArtCreateUpdateDTO>
        {
            new ArtCreateUpdateDTO { Id = 1, Title = "Art 1", Description = "Desc 1", ImageId = 999, ModelState = ModelState.Created }
        };
        request.NewStreetcode.StreetcodeArtSlides = new List<StreetcodeArtSlideCreateUpdateDTO>
        {
            new StreetcodeArtSlideCreateUpdateDTO
            {
                Index = 0,
                Template = GallerySlideTemplate.OneToTwo,
                StreetcodeArts = new List<StreetcodeArtCreateUpdateDTO>
                {
                    new StreetcodeArtCreateUpdateDTO { Index = 0, ArtId = 1 },
                }
            }
        };
        return request;
    }

    private StreetcodeCreateCommand CreateRequestWithInvalidArtIdInSlide()
    {
        var request = CreateValidRequest();
        request.NewStreetcode.StreetcodeArtSlides = new List<StreetcodeArtSlideCreateUpdateDTO>
        {
            new StreetcodeArtSlideCreateUpdateDTO
            {
                Index = 0,
                Template = GallerySlideTemplate.OneToTwo,
                StreetcodeArts = new List<StreetcodeArtCreateUpdateDTO>
                {
                    new StreetcodeArtCreateUpdateDTO { Index = 0, ArtId = 999 }
                }
            }
        };
        return request;
    }

    private StreetcodeCreateCommand CreateRequestWithEmptyImages()
    {
        var request = CreateValidRequest();
        request.NewStreetcode.ImagesDetails = new List<ImageDetailsDto>();
        return request;
    }

    private StreetcodeCreateCommand CreateRequestWithInvalidImageIds()
    {
        var request = CreateValidRequest();
        request.NewStreetcode.ImagesDetails = new List<ImageDetailsDto>
        {
            new ImageDetailsDto { ImageId = 0, Alt = "Invalid" },
            new ImageDetailsDto { ImageId = -1, Alt = "Invalid" }
        };
        return request;
    }

    private StreetcodeCreateCommand CreateRequestWithEmptyTags()
    {
        var request = CreateValidRequest();
        request.NewStreetcode.Tags = new List<StreetcodeTagDTO>();
        return request;
    }

    private StreetcodeContent CreateStreetcodeEntity()
    {
        return new StreetcodeContent
        {
            Id = 1,
            Title = "Test Streetcode",
            TransliterationUrl = "test-streetcode",
            Index = 1
        };
    }

    private StreetcodeDTO CreateStreetcodeDTO()
    {
        return new StreetcodeDTO
        {
            Id = 1,
            Title = "Test Streetcode",
            TransliterationUrl = "test-streetcode",
            Index = 1
        };
    }

    private void SetupMocksForCompleteSuccess(StreetcodeCreateCommand request, StreetcodeContent entity, StreetcodeDTO dto)
    {
        SetupMocksForInitialSave(request, entity);
        SetupImagesForSuccess();
        SetupTagsForSuccess();
        SetupImagesDetailsForSuccess();

        _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        _mockMapper.Setup(m => m.Map<StreetcodeDTO>(entity))
            .Returns(dto);
    }

    private void SetupMocksForArtGallerySuccess(StreetcodeCreateCommand request, StreetcodeContent entity, StreetcodeDTO dto)
    {
        SetupMocksForInitialSave(request, entity);
        SetupImagesForSuccess();
        SetupTagsForSuccess();
        SetupImagesDetailsForSuccess();
        SetupArtCreationForSuccess(
            new List<Art>
            {
                new Art { Id = 1, Title = "Art 1", ImageId = 1 },
                new Art { Id = 2, Title = "Art 2", ImageId = 2 },
                new Art { Id = 3, Title = "Art 3", ImageId = 3 }
            },
            [
                new() { Id = 1 },
                new() { Id = 2 }
            ]);
        SetupArtSlideCreationForSuccess();

        _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        _mockMapper.Setup(m => m.Map<StreetcodeDTO>(entity))
            .Returns(dto);
    }

    private void SetupMocksForFilteredArtsSuccess(StreetcodeCreateCommand request, StreetcodeContent entity, StreetcodeDTO dto)
    {
        SetupMocksForInitialSave(request, entity);
        SetupImagesForSuccess();
        SetupTagsForSuccess();
        SetupImagesDetailsForSuccess();
        SetupArtCreationForSuccess(
            new List<Art>
            {
                new Art { Id = 1, Title = "Art 1", ImageId = 1 },
                new Art { Id = 2, Title = "Art 2", ImageId = 2 },
            },
            [
                new() { Id = 1 },
                new() { Id = 2 }
            ]);
        SetupArtSlideCreationForSuccess();

        _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        _mockMapper.Setup(m => m.Map<StreetcodeDTO>(entity))
            .Returns(dto);
    }

    private void SetupMocksForMultipleSlidesSuccess(StreetcodeCreateCommand request, StreetcodeContent entity, StreetcodeDTO dto)
    {
        SetupMocksForInitialSave(request, entity);
        SetupImagesForSuccess();
        SetupTagsForSuccess();
        SetupImagesDetailsForSuccess();
        SetupArtCreationForSuccess(
            new List<Art>
            {
                new Art { Id = 1, Title = "Art 1", ImageId = 1 },
                new Art { Id = 2, Title = "Art 2", ImageId = 2 },
                new Art { Id = 3, Title = "Art 3", ImageId = 3 }
            },
            [
                new() { Id = 1 },
                new() { Id = 2 },
                new() { Id = 3 }
            ]);
        SetupArtSlideCreationForSuccess();

        _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        _mockMapper.Setup(m => m.Map<StreetcodeDTO>(entity))
            .Returns(dto);
    }

    private void SetupMocksForInitialSave(StreetcodeCreateCommand request, StreetcodeContent entity)
    {
        _mockMapper.Setup(m => m.Map<StreetcodeContent>(request.NewStreetcode))
            .Returns(entity);

        _mockRepositoryWrapper.Setup(r => r.StreetcodeRepository.CreateAsync(entity));
        _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);
        _mockRepositoryWrapper.Setup(r => r.BeginTransaction())
            .Returns(new TransactionScope(TransactionScopeOption.Suppress));
    }

    private void SetupImagesForSuccess()
    {
        _mockRepositoryWrapper.Setup(r => r.StreetcodeImageRepository.CreateRangeAsync(It.IsAny<IEnumerable<StreetcodeImage>>()))
            .Returns(Task.CompletedTask);
    }

    private void SetupTagsForSuccess()
    {
        _mockRepositoryWrapper.Setup(r => r.StreetcodeTagIndexRepository.CreateRangeAsync(It.IsAny<IEnumerable<StreetcodeTagIndex>>()))
            .Returns(Task.CompletedTask);
        _mockRepositoryWrapper.Setup(r => r.TagRepository.GetFirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Tag, bool>>>(),
                It.IsAny<Func<IQueryable<Tag>, IIncludableQueryable<Tag, object>>>()))
            .ReturnsAsync((Tag?)null);
        _mockMapper.Setup(m => m.Map<Tag>(It.IsAny<StreetcodeTagDTO>()))
            .Returns(new Tag { Id = 1, Title = "Test Tag" });
    }

    private void SetupImagesDetailsForSuccess()
    {
        _mockRepositoryWrapper.Setup(r => r.ImageDetailsRepository.CreateRangeAsync(It.IsAny<IEnumerable<ImageDetails>>()))
            .Returns(Task.CompletedTask);
        _mockMapper.Setup(m => m.Map<IEnumerable<ImageDetails>>(It.IsAny<IEnumerable<ImageDetailsDto>>()))
            .Returns(new List<ImageDetails> { new ImageDetails { Id = 1, ImageId = 1 } });
    }

    private void SetupArtCreationForSuccess(List<Art> artEntities, List<Image> images)
    {
        // Setup image existence check
        _mockRepositoryWrapper.Setup(r => r.ImageRepository.GetAllAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Image, bool>>>(),
                It.IsAny<Func<IQueryable<Image>, IIncludableQueryable<Image, object>>>()))
            .ReturnsAsync(images);

        _mockMapper.Setup(m => m.Map<List<Art>>(It.IsAny<List<ArtCreateUpdateDTO>>()))
            .Returns(artEntities);

        _mockRepositoryWrapper.Setup(r => r.ArtRepository.CreateRangeAsync(It.IsAny<IEnumerable<Art>>()))
            .Callback<IEnumerable<Art>>(arts =>
            {
                // Simulate setting IDs for created entities
                var artList = arts.ToList();
                for (int i = 0; i < artList.Count; i++)
                {
                    artList[i].Id = i + 1;
                }
            })
            .Returns(Task.CompletedTask);
    }

    private void SetupArtSlideCreationForSuccess()
    {
        var artSlideEntities = new List<StreetcodeArtSlide>
        {
            new StreetcodeArtSlide { Id = 1, Index = 0, StreetcodeId = 1 },
            new StreetcodeArtSlide { Id = 2, Index = 1, StreetcodeId = 1 }
        };

        _mockMapper.Setup(m => m.Map<List<StreetcodeArtSlide>>(It.IsAny<List<StreetcodeArtSlideCreateUpdateDTO>>()))
            .Returns(artSlideEntities);

        _mockRepositoryWrapper.Setup(r => r.StreetcodeArtSlideRepository.CreateRangeAsync(It.IsAny<IEnumerable<StreetcodeArtSlide>>()))
            .Callback<IEnumerable<StreetcodeArtSlide>>(slides =>
            {
                // Simulate setting IDs for created entities
                var slideList = slides.ToList();
                for (int i = 0; i < slideList.Count; i++)
                {
                    slideList[i].Id = i + 1;
                }
            })
            .Returns(Task.CompletedTask);

        // Setup art existence check for slides
        _mockRepositoryWrapper.Setup(r => r.ArtRepository.GetFirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Art, bool>>>(),
                It.IsAny<Func<IQueryable<Art>, IIncludableQueryable<Art, object>>>()))
            .ReturnsAsync(new Art { Id = 1 });

        _mockMapper.Setup(m => m.Map<StreetcodeArt>(It.IsAny<StreetcodeArtCreateUpdateDTO>()))
            .Returns(new StreetcodeArt());

        _mockRepositoryWrapper.Setup(r => r.StreetcodeArtRepository.CreateRangeAsync(It.IsAny<IEnumerable<StreetcodeArt>>()))
            .Returns(Task.CompletedTask);
    }

    private void VerifySuccessCalls(StreetcodeCreateCommand request, StreetcodeContent entity)
    {
        _mockMapper.Verify(m => m.Map<StreetcodeContent>(request.NewStreetcode), Times.Once);
        _mockRepositoryWrapper.Verify(r => r.StreetcodeRepository.CreateAsync(entity), Times.Once);
        _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.AtLeast(2));
        _mockMapper.Verify(m => m.Map<StreetcodeDTO>(entity), Times.Once);
        _mockLogger.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Once);
    }

    private void VerifyArtGalleryCreationCalls()
    {
        _mockRepositoryWrapper.Verify(r => r.ArtRepository.CreateRangeAsync(It.IsAny<IEnumerable<Art>>()), Times.Once);
        _mockRepositoryWrapper.Verify(r => r.StreetcodeArtSlideRepository.CreateRangeAsync(It.IsAny<IEnumerable<StreetcodeArtSlide>>()), Times.Once);
        _mockRepositoryWrapper.Verify(r => r.StreetcodeArtRepository.CreateRangeAsync(It.IsAny<IEnumerable<StreetcodeArt>>()), Times.Once);
    }
}