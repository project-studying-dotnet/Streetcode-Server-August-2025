using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Media.Art;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Media.Art.GetAll;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;
using ArtEntity = Streetcode.DAL.Entities.Media.Images.Art;

namespace Streetcode.XUnitTest.BLL_Tests.MediatR.Media.Art.GetAll
{
    public class GetAllArtsHandlerTests
    {
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly GetAllArtsHandler _handler;

        public GetAllArtsHandlerTests()
        {
            _mapperMock = new Mock<IMapper>();
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _loggerMock = new Mock<ILoggerService>();
            _handler = new GetAllArtsHandler(_repositoryWrapperMock.Object, _mapperMock.Object, _loggerMock.Object);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(10)]
        public async Task Handle_WithDifferentListSizes_ReturnsSuccess(int count)
        {
            // Arrange
            var arts = Enumerable.Range(1, count)
                .Select(i => new ArtEntity { Id = i, Title = $"Art{i}" })
                .ToList();

            var artsDto = arts.Select(a => new ArtDTO { Id = a.Id, Title = a.Title }).ToList();

            _repositoryWrapperMock.Setup(r => r.ArtRepository.GetAllAsync(
                It.IsAny<Expression<Func<ArtEntity, bool>>>(),
                It.IsAny<Func<IQueryable<ArtEntity>, IIncludableQueryable<ArtEntity, object>>>()))
                .ReturnsAsync(arts);

            _mapperMock.Setup(m => m.Map<IEnumerable<ArtDTO>>(arts)).Returns(artsDto);

            var query = new GetAllArtsQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(artsDto, result.Value);

            _repositoryWrapperMock.Verify(r => r.ArtRepository.GetAllAsync(
                It.IsAny<Expression<Func<ArtEntity, bool>>>(),
                It.IsAny<Func<IQueryable<ArtEntity>, IIncludableQueryable<ArtEntity, object>>>()), Times.Once);

            _mapperMock.Verify(m => m.Map<IEnumerable<ArtDTO>>(arts), Times.Once);
            _loggerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_WhenArtsIsNull_ReturnsFailAndLogsError()
        {
            // Arrange
            const string ErrorMessage = "Cannot find any arts";

            _repositoryWrapperMock.Setup(r => r.ArtRepository.GetAllAsync(
                It.IsAny<Expression<Func<ArtEntity, bool>>>(),
                It.IsAny<Func<IQueryable<ArtEntity>, IIncludableQueryable<ArtEntity, object>>>()))
                .ReturnsAsync((IEnumerable<ArtEntity>?)null);

            var query = new GetAllArtsQuery();

            // Act
            var result = await _handler.Handle(query, default);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(result.Errors, e => e.Message.Contains(ErrorMessage));

            _repositoryWrapperMock.Verify(r => r.ArtRepository.GetAllAsync(
                It.IsAny<Expression<Func<ArtEntity, bool>>>(),
                It.IsAny<Func<IQueryable<ArtEntity>, IIncludableQueryable<ArtEntity, object>>>()), Times.Once);

            _loggerMock.Verify(l => l.LogError(query, It.Is<string>(s => s.Contains(ErrorMessage))), Times.Once);
            _mapperMock.VerifyNoOtherCalls();
        }
    }
}
