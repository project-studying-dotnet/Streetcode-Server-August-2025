using System.Linq.Expressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Sources;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Sources.SourceLinkCategory.GetAll;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;
using SourceLinkCategoryEntity = Streetcode.DAL.Entities.Sources.SourceLinkCategory;

namespace Streetcode.XUnitTest.BLL.MediatR.Sources.SourceLinkCategory.GetAll
{
    public class GetAllCategoryNamesHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly GetAllCategoryNamesHandler _handler;

        public GetAllCategoryNamesHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _loggerMock = new Mock<ILoggerService>();
            _mapperMock = new Mock<IMapper>();
            _handler = new GetAllCategoryNamesHandler(_repositoryWrapperMock.Object, _mapperMock.Object, _loggerMock.Object);
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
            var categories = Enumerable.Range(1, count)
                .Select(i => new SourceLinkCategoryEntity { Id = i, Title = $"SourceLinkCategory{i}" })
                .ToList();

            var categoryDtos = categories.Select(c => new CategoryWithNameDTO { Id = c.Id, Title = c.Title })
                .ToList();

            _repositoryWrapperMock.Setup(r => r.SourceCategoryRepository.GetAllAsync(
                It.IsAny<Expression<Func<SourceLinkCategoryEntity, bool>>>(),
                It.IsAny<Func<IQueryable<SourceLinkCategoryEntity>, IIncludableQueryable<SourceLinkCategoryEntity, object>>>()))
                .ReturnsAsync(categories);

            _mapperMock.Setup(m => m.Map<IEnumerable<CategoryWithNameDTO>>(categories)).Returns(categoryDtos);

            var query = new GetAllCategoryNamesQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(categoryDtos, result.Value);

            _repositoryWrapperMock.Verify(
                r => r.SourceCategoryRepository.GetAllAsync(
                    It.IsAny<Expression<Func<SourceLinkCategoryEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<SourceLinkCategoryEntity>, IIncludableQueryable<SourceLinkCategoryEntity, object>>>()),
                Times.Once);

            _mapperMock.Verify(m => m.Map<IEnumerable<CategoryWithNameDTO>>(categories), Times.Once);
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
                .ReturnsAsync((IEnumerable<SourceLinkCategoryEntity>?)null);

            var query = new GetAllCategoryNamesQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(errorMsg, result.Errors[0].Message);

            _loggerMock.Verify(l => l.LogError(It.IsAny<GetAllCategoryNamesQuery>(), errorMsg), Times.Once);
            _repositoryWrapperMock.Verify(
                r => r.SourceCategoryRepository.GetAllAsync(
                    It.IsAny<Expression<Func<SourceLinkCategoryEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<SourceLinkCategoryEntity>, IIncludableQueryable<SourceLinkCategoryEntity, object>>>()),
                Times.Once);
            _mapperMock.VerifyNoOtherCalls();
        }
    }
}
