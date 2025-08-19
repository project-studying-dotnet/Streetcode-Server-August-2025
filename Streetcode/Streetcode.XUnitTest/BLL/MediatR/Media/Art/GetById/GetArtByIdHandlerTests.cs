using System.Linq.Expressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Media.Art;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Media.Art.GetById;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;
using ArtEntity = Streetcode.DAL.Entities.Media.Images.Art;

namespace Streetcode.XUnitTest.BLL_Tests.MediatR.Media.Art.GetById
{
    public class GetArtByIdHandlerTests
    {
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly GetArtByIdHandler _handler;

        public GetArtByIdHandlerTests()
        {
            _mapperMock = new Mock<IMapper>();
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _loggerMock = new Mock<ILoggerService>();
            _handler = new GetArtByIdHandler(_repositoryWrapperMock.Object, _mapperMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_WhenArtExists_ReturnsCorrectArt()
        {
            // Arrange
            const int targetArtId = 2;
            var arts = new List<ArtEntity>
            {
                new ArtEntity { Id = 1, Title = "Art1" },
                new ArtEntity { Id = 2, Title = "Art2" },
                new ArtEntity { Id = 3, Title = "Art3" },
            };

            var targetArt = arts.First(a => a.Id == targetArtId);
            var targetArtDto = new ArtDTO { Id = targetArt.Id, Title = targetArt.Title };

            _repositoryWrapperMock
                .Setup(r => r.ArtRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<ArtEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<ArtEntity>, IIncludableQueryable<ArtEntity, object>>>()))
                .ReturnsAsync((Expression<Func<ArtEntity, bool>> predicate, Func<IQueryable<ArtEntity>,
                    IIncludableQueryable<ArtEntity, object>> _) =>
                    arts.AsQueryable().FirstOrDefault(predicate.Compile()));

            _mapperMock.Setup(m => m.Map<ArtDTO>(targetArt)).Returns(targetArtDto);

            var query = new GetArtByIdQuery(targetArtId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(targetArtDto, result.Value);

            _repositoryWrapperMock.Verify(
                r => r.ArtRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<ArtEntity, bool>>>(),
                It.IsAny<Func<IQueryable<ArtEntity>, IIncludableQueryable<ArtEntity, object>>>()), Times.Once);

            _mapperMock.Verify(m => m.Map<ArtDTO>(targetArt), Times.Once);
            _loggerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_WhenArtDoesNotExist_ReturnsFailAndLogsError()
        {
            // Arrange
            const int nonExistentArtId = -1;
            var arts = new List<ArtEntity>
            {
                new ArtEntity { Id = 1, Title = "Art1" },
                new ArtEntity { Id = 2, Title = "Art2" },
                new ArtEntity { Id = 3, Title = "Art3" },
            };

            _repositoryWrapperMock
                .Setup(r => r.ArtRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<ArtEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<ArtEntity>, IIncludableQueryable<ArtEntity, object>>>()))
                .ReturnsAsync((Expression<Func<ArtEntity, bool>> predicate, Func<IQueryable<ArtEntity>,
                    IIncludableQueryable<ArtEntity, object>> _) =>
                    arts.AsQueryable().FirstOrDefault(predicate.Compile()));

            var query = new GetArtByIdQuery(nonExistentArtId);
            var expectedMessage = $"Cannot find an art with corresponding id: {nonExistentArtId}";

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(expectedMessage, result.Errors[0].Message);

            _repositoryWrapperMock.Verify(
                r => r.ArtRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<ArtEntity, bool>>>(),
                It.IsAny<Func<IQueryable<ArtEntity>, IIncludableQueryable<ArtEntity, object>>>()), Times.Once);

            _mapperMock.Verify(m => m.Map<ArtDTO>(It.IsAny<ArtEntity>()), Times.Never);
            _loggerMock.Verify(l => l.LogError(query, expectedMessage), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenArtIsNull_ReturnsFailAndLogsError()
        {
            // Arrange
            _repositoryWrapperMock
              .Setup(r => r.ArtRepository.GetFirstOrDefaultAsync(
                  It.IsAny<Expression<Func<ArtEntity, bool>>>(),
                  It.IsAny<Func<IQueryable<ArtEntity>, IIncludableQueryable<ArtEntity, object>>>()))
              .ReturnsAsync((ArtEntity)null);

            var query = new GetArtByIdQuery(1);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);
            var message = $"Cannot find an art with corresponding id: {query.Id}";

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(message, result.Errors[0].Message);
            _loggerMock.Verify(l => l.LogError(query, message), Times.Once);
            _mapperMock.Verify(m => m.Map<ArtDTO>(It.IsAny<ArtEntity>()), Times.Never);
        }
    }
}
