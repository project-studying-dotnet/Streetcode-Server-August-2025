using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Streetcode.TextContent.Text;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.Text.GetAll;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;
using TextEntity = Streetcode.DAL.Entities.Streetcode.TextContent.Text;

namespace Streetcode.XUnitTest.BLL.MediatR.StreetCode.Text.GetAll
{
    public class GetAllTextsHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILoggerService> _loggerServiceMock;
        private readonly GetAllTextsHandler _handler;
        public GetAllTextsHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _loggerServiceMock = new Mock<ILoggerService>();

            _handler = new GetAllTextsHandler(_repositoryWrapperMock.Object, _mapperMock.Object, _loggerServiceMock.Object);
        }

        [Fact]
        public async Task Handle_WhenTextsExist_ReturnsOkResultWithMappedDtos()
        {
            // Arrange
            var entityList = new List<TextEntity> { new TextEntity { Id = 1, Title = "Test title" } };
            var dtoList = new List<TextDTO> { new TextDTO { Id = 1, Title = "Test title" } };

            _repositoryWrapperMock.Setup(r => r.TextRepository.GetAllAsync(
                It.IsAny<Expression<Func<TextEntity, bool>>>(),
                It.IsAny<Func<IQueryable<TextEntity>, IIncludableQueryable<TextEntity, object>>>()))
            .ReturnsAsync(entityList);

            _mapperMock.Setup(m => m.Map<IEnumerable<TextDTO>>(entityList))
                .Returns(dtoList);

            var query = new GetAllTextsQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(dtoList, result.Value);
            _repositoryWrapperMock.Verify(
                r => r.TextRepository.GetAllAsync(
                It.IsAny<Expression<Func<TextEntity, bool>>>(),
                It.IsAny<Func<IQueryable<TextEntity>, IIncludableQueryable<TextEntity, object>>>()),
                Times.Once);
            _mapperMock.Verify(m => m.Map<IEnumerable<TextDTO>>(entityList), Times.Once);
            _loggerServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_WhenTextsAreNull_ReturnsFailAndLogsError()
        {
            // Arrange
            _repositoryWrapperMock.Setup(r => r.TextRepository.GetAllAsync(It.IsAny<Expression<Func<TextEntity, bool>>>(), It.IsAny<Func<IQueryable<TextEntity>, IIncludableQueryable<TextEntity, object>>>()))
                .ReturnsAsync((IEnumerable<TextEntity>)null);

            var query = new GetAllTextsQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(result.Errors, e => e.Message.Contains("Cannot find any text"));
            _loggerServiceMock.Verify(l => l.LogError(query, It.Is<string>(s => s.Contains("Cannot find any text"))), Times.Once);
            _mapperMock.VerifyNoOtherCalls();
        }
    }
}
