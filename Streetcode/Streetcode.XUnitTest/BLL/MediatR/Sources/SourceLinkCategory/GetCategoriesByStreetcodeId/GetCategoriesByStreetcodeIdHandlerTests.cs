using System.Linq.Expressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Media.Images;
using Streetcode.BLL.DTO.Sources;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Sources.SourceLink.GetCategoriesByStreetcodeId;
using Streetcode.DAL.Entities.Media.Images;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;
using SourceLinkCategoryEntity = Streetcode.DAL.Entities.Sources.SourceLinkCategory;

namespace Streetcode.XUnitTest.BLL.MediatR.Sources.SourceLinkCategory.GetCategoriesByStreetcodeId
{
    public class GetCategoriesByStreetcodeIdHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IBlobService> _blobServiceMock;
        private readonly GetCategoriesByStreetcodeIdHandler _handler;

        public GetCategoriesByStreetcodeIdHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _loggerMock = new Mock<ILoggerService>();
            _mapperMock = new Mock<IMapper>();
            _blobServiceMock = new Mock<IBlobService>();
            _handler = new GetCategoriesByStreetcodeIdHandler(_repositoryWrapperMock.Object, _mapperMock.Object, _blobServiceMock.Object, _loggerMock.Object);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(1, 1)]
        [InlineData(50, 50)]
        [InlineData(100, 100)]
        public async Task Handle_WithDifferentCategoriesCount_ReturnsSuccess(int count, int streetcodeId)
        {
            // Arrange
            var (categories, categoryDtos) = CreateCategoriesAndDtos(count);

            _repositoryWrapperMock.Setup(r => r.SourceCategoryRepository.GetAllAsync(
               It.IsAny<Expression<Func<SourceLinkCategoryEntity, bool>>>(),
               It.IsAny<Func<IQueryable<SourceLinkCategoryEntity>, IIncludableQueryable<SourceLinkCategoryEntity, object>>>()))
               .ReturnsAsync(categories);

            _mapperMock.Setup(m => m.Map<IEnumerable<SourceLinkCategoryDTO>>(categories)).Returns(categoryDtos);
            SetupBlobServiceMocks(categoryDtos);

            var query = new GetCategoriesByStreetcodeIdQuery(streetcodeId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(count, result.Value.Count());
            Assert.Equal(categoryDtos, result.Value);

            VerifyBlobServiceMocks(categoryDtos);

            _repositoryWrapperMock.Verify(
                r => r.SourceCategoryRepository.GetAllAsync(
                It.IsAny<Expression<Func<SourceLinkCategoryEntity, bool>>>(),
                It.IsAny<Func<IQueryable<SourceLinkCategoryEntity>,
                    IIncludableQueryable<SourceLinkCategoryEntity, object>>>()), Times.Once);

            _mapperMock.Verify(m => m.Map<IEnumerable<SourceLinkCategoryDTO>>(categories), Times.Once);
            _loggerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task When_CategoriesIsNull_ReturnsFailAndLogsError()
        {
            // Arrange
            int streetcodeId = 1;
            string expectedErrorMessage = $"Cant find any source category with the streetcode id {streetcodeId}";

            _repositoryWrapperMock.Setup(r => r.SourceCategoryRepository.GetAllAsync(
                It.IsAny<Expression<Func<SourceLinkCategoryEntity, bool>>>(),
                It.IsAny<Func<IQueryable<SourceLinkCategoryEntity>, IIncludableQueryable<SourceLinkCategoryEntity, object>>>()))
                .ReturnsAsync((IEnumerable<SourceLinkCategoryEntity>?)null);

            var query = new GetCategoriesByStreetcodeIdQuery(streetcodeId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(result.Errors, e => e.Message == expectedErrorMessage);

            _repositoryWrapperMock.Verify(
                r => r.SourceCategoryRepository.GetAllAsync(
                It.IsAny<Expression<Func<SourceLinkCategoryEntity, bool>>>(),
                It.IsAny<Func<IQueryable<SourceLinkCategoryEntity>,
                    IIncludableQueryable<SourceLinkCategoryEntity, object>>>()), Times.Once);

            _loggerMock.Verify(l => l.LogError(It.IsAny<GetCategoriesByStreetcodeIdQuery>(), expectedErrorMessage), Times.Once);
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
