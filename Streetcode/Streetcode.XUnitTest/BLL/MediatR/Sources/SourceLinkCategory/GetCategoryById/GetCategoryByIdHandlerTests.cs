using System.Linq.Expressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Sources;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Sources.SourceLink.GetCategoryById;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;
using SourceLinkCategoryEntity = Streetcode.DAL.Entities.Sources.SourceLinkCategory;

namespace Streetcode.XUnitTest.BLL.MediatR.Sources.SourceLinkCategory.GetCategoryById
{
    public class GetCategoryByIdHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IBlobService> _blobServiceMock;
        private readonly GetCategoryByIdHandler _handler;

        public GetCategoryByIdHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _loggerMock = new Mock<ILoggerService>();
            _mapperMock = new Mock<IMapper>();
            _blobServiceMock = new Mock<IBlobService>();
            _handler = new GetCategoryByIdHandler(_repositoryWrapperMock.Object, _mapperMock.Object, _blobServiceMock.Object, _loggerMock.Object);
        }

        public async Task Handle_WithValidId_ReturnsSuccess()
        {
            // Arrange
            int categoryId = 1;
            var categoryEntity = new SourceLinkCategoryEntity { Id = categoryId };
            var categoryDto = new SourceLinkCategoryDTO { Id = categoryId };

            _repositoryWrapperMock.Setup(r => r.SourceCategoryRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<SourceLinkCategoryEntity, bool>>>(),
                It.IsAny<Func<IQueryable<SourceLinkCategoryEntity>,
                    IIncludableQueryable<SourceLinkCategoryEntity, object>>>()))
                .ReturnsAsync(categoryEntity);

            _mapperMock.Setup(m => m.Map<SourceLinkCategoryDTO>(categoryEntity)).Returns(categoryDto);

            _blobServiceMock.Setup(b => b.FindFileInStorageAsBase64(It.IsAny<string>())).Returns("base64string");

            var query = new GetCategoryByIdQuery(categoryId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(categoryDto, result.Value);

            _repositoryWrapperMock.Verify(
                r => r.SourceCategoryRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<SourceLinkCategoryEntity, bool>>>(),
                It.IsAny<Func<IQueryable<SourceLinkCategoryEntity>,
                    IIncludableQueryable<SourceLinkCategoryEntity, object>>>()), Times.Once);

            _mapperMock.Verify(m => m.Map<SourceLinkCategoryDTO>(categoryEntity), Times.Once);
            _blobServiceMock.Verify(b => b.FindFileInStorageAsBase64(It.IsAny<string>()), Times.Once);
            _loggerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_WithInvalidId_ReturnsFailureAndLogsError()
        {
            // Arrange
            int categoryId = -1;
            string errorMsg = $"Cannot find any srcCategory by the corresponding id: {categoryId}";
            _repositoryWrapperMock.Setup(r => r.SourceCategoryRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<SourceLinkCategoryEntity, bool>>>(),
                It.IsAny<Func<IQueryable<SourceLinkCategoryEntity>,
                    IIncludableQueryable<SourceLinkCategoryEntity, object>>>()))
                .ReturnsAsync((SourceLinkCategoryEntity)null!);

            var query = new GetCategoryByIdQuery(categoryId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(result.Errors, e => e.Message.Contains(errorMsg));

            _repositoryWrapperMock.Verify(
                r => r.SourceCategoryRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<SourceLinkCategoryEntity, bool>>>(),
                It.IsAny<Func<IQueryable<SourceLinkCategoryEntity>,
                    IIncludableQueryable<SourceLinkCategoryEntity, object>>>()), Times.Once);

            _loggerMock.Verify(l => l.LogError(It.IsAny<GetCategoryByIdQuery>(), It.Is<string>(s => s.Contains(errorMsg))), Times.Once);
            _mapperMock.VerifyNoOtherCalls();
            _blobServiceMock.VerifyNoOtherCalls();
        }
    }
}
