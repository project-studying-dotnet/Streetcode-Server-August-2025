using System.Linq.Expressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Sources;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Sources.SourceLinkCategory.GetCategoryContentByStreetcodeId;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;
using StreetcodeCategoryContentEntity = Streetcode.DAL.Entities.Sources.StreetcodeCategoryContent;

namespace Streetcode.XUnitTest.BLL.MediatR.Sources.SourceLinkCategory.GetCategoryContentByStreetcodeId
{
    public class GetCategoryContentByStreetcodeIdHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly GetCategoryContentByStreetcodeIdHandler _handler;

        public GetCategoryContentByStreetcodeIdHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILoggerService>();
            _handler = new GetCategoryContentByStreetcodeIdHandler(_repositoryWrapperMock.Object, _mapperMock.Object, _loggerMock.Object);
        }

        public async Task Handle_WhenCategoryContentExist_ReturnsSuccess()
        {
            // Arrange
            int streetcodeId = 5;
            var category = new StreetcodeCategoryContentEntity { SourceLinkCategoryId = 1, StreetcodeId = streetcodeId };

            var categoryContentDto = new StreetcodeCategoryContentDTO
            {
                SourceLinkCategoryId = category.SourceLinkCategoryId,
                StreetcodeId = category.StreetcodeId,
            };

            _repositoryWrapperMock.Setup(r => r.StreetcodeCategoryContentRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<StreetcodeCategoryContentEntity, bool>>>(),
                It.IsAny<Func<IQueryable<StreetcodeCategoryContentEntity>, IIncludableQueryable<StreetcodeCategoryContentEntity, object>>>()))
                .ReturnsAsync(category);

            _repositoryWrapperMock.Setup(r => r.StreetcodeRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<StreetcodeContent, bool>>>(), null))
                .ReturnsAsync(new StreetcodeContent { Id = streetcodeId });

            _mapperMock.Setup(m => m.Map<StreetcodeCategoryContentDTO>(It.IsAny<StreetcodeCategoryContentEntity>()));

            var query = new GetCategoryContentByStreetcodeIdQuery(streetcodeId, 1);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(categoryContentDto, result.Value);

            _repositoryWrapperMock.Verify(
                r => r.StreetcodeCategoryContentRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<StreetcodeCategoryContentEntity, bool>>>(),
                It.IsAny<Func<IQueryable<StreetcodeCategoryContentEntity>,
                    IIncludableQueryable<StreetcodeCategoryContentEntity, object>>>()), Times.Once);

            _repositoryWrapperMock.Verify(
                r => r.StreetcodeRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<StreetcodeContent, bool>>>(), null), Times.Once);

            _mapperMock.Verify(
                m => m.Map<StreetcodeCategoryContentDTO>(
                It.IsAny<StreetcodeCategoryContentEntity>()), Times.Once);

            _loggerMock.VerifyNoOtherCalls();
        }

        public async Task Handle_WhenCategoryContentDoesNotExist_ReturnsFailAndLogsError()
        {
            // Arrange
            const string errorMsg = "The streetcode content is null";
            const int streetcodeId = 5, categoryId = 1;

            _repositoryWrapperMock.Setup(r => r.StreetcodeCategoryContentRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<StreetcodeCategoryContentEntity, bool>>>(),
                It.IsAny<Func<IQueryable<StreetcodeCategoryContentEntity>, IIncludableQueryable<StreetcodeCategoryContentEntity, object>>>()))
                .ReturnsAsync((StreetcodeCategoryContentEntity?)null);

            _repositoryWrapperMock.Setup(r => r.StreetcodeRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<StreetcodeContent, bool>>>(), null))
                .ReturnsAsync(new StreetcodeContent { Id = streetcodeId });

            var query = new GetCategoryContentByStreetcodeIdQuery(streetcodeId, categoryId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(errorMsg, result.Errors[0].Message);

            _repositoryWrapperMock.Verify(
                r => r.StreetcodeCategoryContentRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<StreetcodeCategoryContentEntity, bool>>>(),
                It.IsAny<Func<IQueryable<StreetcodeCategoryContentEntity>,
                    IIncludableQueryable<StreetcodeCategoryContentEntity, object>>>()), Times.Once);

            _repositoryWrapperMock.Verify(
                r => r.StreetcodeRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<StreetcodeContent, bool>>>(), null), Times.Once);

            _loggerMock.Verify(l => l.LogError(It.IsAny<object>(), errorMsg), Times.Once);

            _mapperMock.VerifyNoOtherCalls();
        }

        public async Task Handle_WhenStreetcodeDoesNotExist_ReturnsFailAndLogsError()
        {
            // Arrange
            int streetcodeId = -10, categoryId = 2;
            string errorMsg = $"No such streetcode with id = {streetcodeId}";
            _repositoryWrapperMock.Setup(r => r.StreetcodeRepository
                .GetFirstOrDefaultAsync(It.IsAny<Expression<Func<StreetcodeContent, bool>>>(), null))
                .ReturnsAsync((StreetcodeContent?)null);

            _repositoryWrapperMock.Setup(r => r.StreetcodeCategoryContentRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<StreetcodeCategoryContentEntity, bool>>>(),
                It.IsAny<Func<IQueryable<StreetcodeCategoryContentEntity>, IIncludableQueryable<StreetcodeCategoryContentEntity, object>>>()))
                .ReturnsAsync((StreetcodeCategoryContentEntity?)null);

            var query = new GetCategoryContentByStreetcodeIdQuery(streetcodeId, categoryId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(errorMsg, result.Errors[0].Message);

            _repositoryWrapperMock.Verify(
                r => r.StreetcodeRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<StreetcodeContent, bool>>>(), null), Times.Once);

            _loggerMock.Verify(l => l.LogError(It.IsAny<object>(), errorMsg), Times.Once);

            _repositoryWrapperMock.Verify(
                r => r.StreetcodeCategoryContentRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<StreetcodeCategoryContentEntity, bool>>>(),
                It.IsAny<Func<IQueryable<StreetcodeCategoryContentEntity>,
                    IIncludableQueryable<StreetcodeCategoryContentEntity, object>>>()), Times.Never);

            _mapperMock.VerifyNoOtherCalls();
        }
    }
}