using System.Linq.Expressions;
using AutoMapper;
using Moq;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Sources.SourceLinkCategory.GetAll;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Entities.Media.Images;
using Xunit;
using Streetcode.BLL.DTO.Sources;
using Streetcode.BLL.DTO.Media.Images;
using Microsoft.EntityFrameworkCore.Query;
using SourceLinkCategoryEntity = Streetcode.DAL.Entities.Sources.SourceLinkCategory;

namespace Streetcode.XUnitTest.BLL.MediatR.Sources.SourceLinkCategory.GetAll
{
    public class GetAllCategoriesHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IBlobService> _blobServiceMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly GetAllCategoriesHandler _handler;

        public GetAllCategoriesHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _blobServiceMock = new Mock<IBlobService>();
            _loggerMock = new Mock<ILoggerService>();
            _handler = new GetAllCategoriesHandler(
                _repositoryWrapperMock.Object,
                _mapperMock.Object,
                _blobServiceMock.Object,
                _loggerMock.Object);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(50)]
        [InlineData(99)]
        [InlineData(100)]
        public async Task Handle_WithDifferentCategoryCounts_ReturnsSuccess(int count)
        {
            // Arrange
            var (categories, categoryDtos) = CreateCategoriesAndDtos(count);

            _repositoryWrapperMock.Setup(r => r.SourceCategoryRepository.GetAllAsync(
                It.IsAny<Expression<Func<SourceLinkCategoryEntity, bool>>>(),
                It.IsAny<Func<IQueryable<SourceLinkCategoryEntity>, IIncludableQueryable<SourceLinkCategoryEntity, object>>>()))
                .ReturnsAsync(categories);

            _mapperMock.Setup(m => m.Map<IEnumerable<SourceLinkCategoryDTO>>(categories)).Returns(categoryDtos);

            SetupBlobServiceMocks(categoryDtos);

            var query = new GetAllCategoriesQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.Value.Count() == count);
            Assert.Equal(categoryDtos, result.Value);

            _repositoryWrapperMock.Verify(
                r => r.SourceCategoryRepository.GetAllAsync(
                    It.IsAny<Expression<Func<SourceLinkCategoryEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<SourceLinkCategoryEntity>, IIncludableQueryable<SourceLinkCategoryEntity, object>>>()),
                Times.Once);
            _mapperMock.Verify(m => m.Map<IEnumerable<SourceLinkCategoryDTO>>(categories), Times.Once);

            VerifyBlobServiceMocks(categoryDtos);

            _loggerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_WhenCategoriesIsNull_ReturnsFailAndLogsError()
        {
            // Arrange
            const string errorMsg = $"Categories is null";

            _repositoryWrapperMock.Setup(r => r.SourceCategoryRepository.GetAllAsync(
                It.IsAny<Expression<Func<SourceLinkCategoryEntity, bool>>>(),
                It.IsAny<Func<IQueryable<SourceLinkCategoryEntity>, IIncludableQueryable<SourceLinkCategoryEntity, object>>>()))
                .ReturnsAsync((List<SourceLinkCategoryEntity>?)null!);

            var query = new GetAllCategoriesQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(result.Errors, e => e.Message == errorMsg);
            _loggerMock.Verify(l => l.LogError(query, errorMsg), Times.Once);
            _repositoryWrapperMock.Verify(
                r => r.SourceCategoryRepository.GetAllAsync(
                It.IsAny<Expression<Func<SourceLinkCategoryEntity, bool>>>(),
                It.IsAny<Func<IQueryable<SourceLinkCategoryEntity>,
                    IIncludableQueryable<SourceLinkCategoryEntity, object>>>()), Times.Once);

            _mapperMock.VerifyNoOtherCalls();
            _blobServiceMock.VerifyNoOtherCalls();
        }

        private (List<SourceLinkCategoryEntity>, List<SourceLinkCategoryDTO>) CreateCategoriesAndDtos(int count)
        {
            var categories = Enumerable.Range(1, count)
                .Select(i => new SourceLinkCategoryEntity
                {
                    Id = i,
                    Title = $"SourceLinkCategory{i}",
                    Image = new Image { Id = i, BlobName = $"blobName{i}" }
                })
                .ToList();

            var categoryDtos = categories.Select(c => new SourceLinkCategoryDTO
            {
                Id = c.Id,
                Title = c.Title!,
                Image = new ImageDTO
                {
                    Id = c.Image!.Id,
                    BlobName = c.Image.BlobName,
                    Base64 = $"base64StringForBlob {c.Image.BlobName}"
                }
            })
            .ToList();

            return (categories, categoryDtos);
        }

        private void SetupBlobServiceMocks(List<SourceLinkCategoryDTO> categoryDtos)
        {
            foreach (var dto in categoryDtos)
            {
                _blobServiceMock.Setup(b => b.FindFileInStorageAsBase64(dto.Image!.BlobName!))
                    .Returns(dto.Image!.Base64!);
            }
        }

        private void VerifyBlobServiceMocks(List<SourceLinkCategoryDTO> categoryDtos)
        {
            foreach (var dto in categoryDtos)
            {
                _blobServiceMock.Verify(b => b.FindFileInStorageAsBase64(dto.Image!.BlobName!), Times.Once);
            }
        }
    }
}
