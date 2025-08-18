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
using Streetcode.BLL.Interfaces.Text;
using Streetcode.BLL.MediatR.Streetcode.Text.GetAll;
using Streetcode.BLL.MediatR.Streetcode.Text.GetByStreetcodeId;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;
using TextEntity = Streetcode.DAL.Entities.Streetcode.TextContent.Text;

namespace Streetcode.XUnitTest.BLL.MediatR.StreetCode.Text.GetByStreetcodeId
{
    public class GetTextByStreetCodeIdTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILoggerService> _loggerServiceMock;
        private readonly GetTextByStreetcodeIdHandler _handler;
        private readonly Mock<ITextService> _textServiceMock;
        public GetTextByStreetCodeIdTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _loggerServiceMock = new Mock<ILoggerService>();
            _textServiceMock = new Mock<ITextService>();

            _handler = new GetTextByStreetcodeIdHandler(_repositoryWrapperMock.Object, _mapperMock.Object, _textServiceMock.Object, _loggerServiceMock.Object);
        }

        [Fact]
        public async Task Handle_WhenTextExists_ReturnOkWithMappedDTO()
        {
            var streetcodeId = 10;
            var textEntity = new TextEntity { Id = 1, Title = "title", StreetcodeId = streetcodeId, TextContent = "content" };
            var textDto = new TextDTO { Id = 1, Title = "title", StreetcodeId = streetcodeId, TextContent = "content" };

            _repositoryWrapperMock.Setup(r => r.TextRepository.GetFirstOrDefaultAsync(
               It.IsAny<Expression<Func<TextEntity, bool>>>(),
               It.IsAny<Func<IQueryable<TextEntity>, IIncludableQueryable<TextEntity, object>>>()))
                .ReturnsAsync(textEntity);

            _textServiceMock.Setup(s => s.AddTermsTag(textEntity.TextContent))
                .ReturnsAsync("processed content");

            _mapperMock.Setup(m => m.Map<TextDTO?>(It.IsAny<TextEntity>()))
            .Returns(textDto);

            var query = new GetTextByStreetcodeIdQuery(streetcodeId);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(textDto, result.Value);

            _mapperMock.Verify(m => m.Map<TextDTO?>(textEntity), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenStreetCodeDoNotExist_ReturnFailAndLogError()
        {
            var streetcodeId = 10;

            _repositoryWrapperMock.Setup(r => r.TextRepository.GetFirstOrDefaultAsync(
               It.IsAny<Expression<Func<TextEntity, bool>>>(),
               It.IsAny<Func<IQueryable<TextEntity>, IIncludableQueryable<TextEntity, object>>>()))
               .ReturnsAsync((TextEntity)null);

            _repositoryWrapperMock.Setup(r => r.StreetcodeRepository.GetFirstOrDefaultAsync(
           It.IsAny<Expression<Func<StreetcodeContent, bool>>>(),
           null))
           .ReturnsAsync((StreetcodeContent?)null);

            var query = new GetTextByStreetcodeIdQuery(streetcodeId);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsFailed);
            Assert.Contains(result.Errors, e => e.Message.Contains(streetcodeId.ToString()));
            _loggerServiceMock.Verify(l => l.LogError(query, It.Is<string>(s => s.Contains("doesn`t exist"))), Times.Once);
            _mapperMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_WhenTextNotFoundButSteetCodeExist_ReturtNullResult()
        {
            var streetcodeId = 10;

            var streetcodeEntity = new StreetcodeContent { Id = streetcodeId };

            _repositoryWrapperMock.Setup(r => r.TextRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<TextEntity, bool>>>(),
            null))
            .ReturnsAsync((TextEntity?)null);

            _repositoryWrapperMock.Setup(r => r.StreetcodeRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<StreetcodeContent, bool>>>(),
                null))
                .ReturnsAsync(streetcodeEntity);

            var query = new GetTextByStreetcodeIdQuery(streetcodeId);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Null(result.Value);
            _loggerServiceMock.VerifyNoOtherCalls();
            _mapperMock.VerifyNoOtherCalls();
        }
    }
}
