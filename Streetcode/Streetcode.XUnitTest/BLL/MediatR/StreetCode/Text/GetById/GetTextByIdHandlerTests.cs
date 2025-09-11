using System.Linq.Expressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Streetcode.TextContent.Text;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.Text.GetById;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;
using TextEntity = Streetcode.DAL.Entities.Streetcode.TextContent.Text;

namespace Streetcode.XUnitTest.BLL.MediatR.StreetCode.Text.GetById
{
    public class GetTextByIdHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILoggerService> _loggerServiceMock;
        private readonly GetTextByIdHandler _handler;
        public GetTextByIdHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _loggerServiceMock = new Mock<ILoggerService>();

            _handler = new GetTextByIdHandler(_repositoryWrapperMock.Object, _mapperMock.Object, _loggerServiceMock.Object);
        }

        [Fact]
        public async Task Handler_WhenTextExists_ReturnOkResultWith()
        {
            var entityList = new TextEntity { Id = 1, Title = "Test title" };
            var dtoList = new TextDTO { Id = 1, Title = "Test title" };

            _repositoryWrapperMock.Setup(r => r.TextRepository.GetFirstOrDefaultAsync(
               It.IsAny<Expression<Func<TextEntity, bool>>>(),
               It.IsAny<Func<IQueryable<TextEntity>, IIncludableQueryable<TextEntity, object>>>()))
                .ReturnsAsync(entityList);

            _mapperMock.Setup(m => m.Map<TextDTO>(entityList))
                .Returns(dtoList);

            var query = new GetTextByIdQuery(1);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(dtoList, result.Value);
            _repositoryWrapperMock.Verify(
                r => r.TextRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<TextEntity, bool>>>(),
                It.IsAny<Func<IQueryable<TextEntity>, IIncludableQueryable<TextEntity, object>>>()),
                Times.Once);
            _mapperMock.Verify(m => m.Map<TextDTO>(entityList), Times.Once);
            _loggerServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handler_WhenTextNull_ReturnFailAndLogError()
        {
            _repositoryWrapperMock.Setup(r => r.TextRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<TextEntity, bool>>>(),
                It.IsAny<Func<IQueryable<TextEntity>, IIncludableQueryable<TextEntity, object>>>()))
                .ReturnsAsync((TextEntity)null!);

            var query = new GetTextByIdQuery(1);
            var errorMsg = Errors_Common.NotFoundById.FormatWith("text", query.Id);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsFailed);
            Assert.Contains(result.Errors, e => e.Message.Contains(errorMsg));

            _loggerServiceMock.Verify(
                l => l.LogError(
                    query,
                    It.Is<string>(s => s.Contains(errorMsg))),
                Times.Once);

            _mapperMock.VerifyNoOtherCalls();
        }
    }
}
