using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Streetcode.TextContent;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.RelatedTerm.Delete;
using Streetcode.DAL.Entities.Streetcode.TextContent;
using System.Linq.Expressions;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

using Entity = Streetcode.DAL.Entities.Streetcode.TextContent.RelatedTerm;

namespace Streetcode.XUnitTest.BLL.MediatR.StreetCode.RelatedTerm
{
    public class DeleteRelatedTermHandlerTest
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILoggerService> _loggerServiceMock;
        private readonly DeleteRelatedTermHandler _handler;

        public DeleteRelatedTermHandlerTest()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _loggerServiceMock = new Mock<ILoggerService>();

            _handler = new DeleteRelatedTermHandler(_repositoryWrapperMock.Object, _mapperMock.Object, _loggerServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnSucceed_WhenRelatedTermIsDeletedSuccessfully()
        {
            var entity = new Entity { Id = 1, Word = "Test", TermId = 1 };
            var relatedTerm = new RelatedTermDTO { Id = 1, Word = "Test", TermId = 1 };
            var request = new DeleteRelatedTermCommand(relatedTerm.Word);

            _repositoryWrapperMock.Setup(r => r.RelatedTermRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<Entity, bool>>>(),
                It.IsAny<Func<IQueryable<Entity>, IIncludableQueryable<Entity, object>>>()))
                .ReturnsAsync(entity);

            _repositoryWrapperMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

            _mapperMock.Setup(m => m.Map<RelatedTermDTO>(entity)).Returns(relatedTerm);

            var result = await _handler.Handle(request, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(result.Value, relatedTerm);
            _repositoryWrapperMock.Verify(r => r.RelatedTermRepository.Delete(entity), Times.Once);
            _repositoryWrapperMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldFail_WhenRelatedTermNotFound()
        {
            var entity = new Entity { Id = 1, Word = "Test", TermId = 1 };
            var relatedTerm = new RelatedTermDTO { Id = 1, Word = "Test", TermId = 1 };
            var request = new DeleteRelatedTermCommand(relatedTerm.Word);

            _repositoryWrapperMock.Setup(r => r.RelatedTermRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<Entity, bool>>>(),
                It.IsAny<Func<IQueryable<Entity>, IIncludableQueryable<Entity, object>>>()))
                .ReturnsAsync((Entity)null);

            var result = await _handler.Handle(request, CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(result.Errors.FirstOrDefault().Message, "Cannot find a related term: Test");
            _loggerServiceMock.Verify(l => l.LogError(request, "Cannot find a related term: Test"), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldFail_WhenSaveChangesFail()
        {
            var entity = new Entity { Id = 1, Word = "Test", TermId = 1 };
            var relatedTerm = new RelatedTermDTO { Id = 1, Word = "Test", TermId = 1 };
            var request = new DeleteRelatedTermCommand(relatedTerm.Word);

            _repositoryWrapperMock.Setup(r => r.RelatedTermRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<Entity, bool>>>(),
                It.IsAny<Func<IQueryable<Entity>, IIncludableQueryable<Entity, object>>>()))
                .ReturnsAsync(entity);

            _repositoryWrapperMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0);

            _mapperMock.Setup(m => m.Map<RelatedTermDTO>(entity)).Returns(relatedTerm);

            var result = await _handler.Handle(request, CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(result.Errors.FirstOrDefault().Message, "Failed to delete a related term");
            _loggerServiceMock.Verify(l => l.LogError(request, "Failed to delete a related term"), Times.Once);
        }
    }
}