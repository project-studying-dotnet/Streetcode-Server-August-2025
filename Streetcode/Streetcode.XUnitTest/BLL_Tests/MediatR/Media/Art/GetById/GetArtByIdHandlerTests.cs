using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Media.Art;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Media.Art.GetById;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
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
        public async Task WhenArtExists_ReturnsOk()
        {
            var art = new ArtEntity { Id = 1, Title = "Art1" };
            var artsDto = new ArtDTO { Id = 1, Title = "Art1" };

            _repositoryWrapperMock
               .Setup(r => r.ArtRepository.GetFirstOrDefaultAsync(
                   It.IsAny<Expression<Func<ArtEntity, bool>>>(),
                   It.IsAny<Func<IQueryable<ArtEntity>, IIncludableQueryable<ArtEntity, object>>>()))
               .ReturnsAsync(art);

            _mapperMock.Setup(m => m.Map<ArtDTO>(art)).Returns(artsDto);

            var query = new GetArtByIdQuery(1);
            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(artsDto, result.Value);

            _repositoryWrapperMock.Verify(r => r.ArtRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<ArtEntity, bool>>>(),
                It.IsAny<Func<IQueryable<ArtEntity>, IIncludableQueryable<ArtEntity, object>>>()),
                Times.Once);

            _mapperMock.Verify(m => m.Map<ArtDTO>(art), Times.Once);
            _loggerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task WhenArtIsNull_ReturnsFailAndLogsError()
        {
            _repositoryWrapperMock
              .Setup(r => r.ArtRepository.GetFirstOrDefaultAsync(
                  It.IsAny<Expression<Func<ArtEntity, bool>>>(),
                  It.IsAny<Func<IQueryable<ArtEntity>, IIncludableQueryable<ArtEntity, object>>>()))
              .ReturnsAsync((ArtEntity)null);

            var query = new GetArtByIdQuery(1);

            var result = await _handler.Handle(query, CancellationToken.None);
            var message = $"Cannot find an art with corresponding id: {query.Id}";

            Assert.False(result.IsSuccess);
            Assert.Equal(message, result.Errors[0].Message);
            _loggerMock.Verify(l => l.LogError(query, message), Times.Once);
            _mapperMock.Verify(m => m.Map<ArtDTO>(It.IsAny<ArtEntity>()), Times.Never);
        }
    }
}
