using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Streetcode.TextContent;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.RelatedTerm.Create;
using System.Linq.Expressions;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

using Entity = Streetcode.DAL.Entities.Streetcode.TextContent.RelatedTerm;

namespace Streetcode.XUnitTest.BLL.MediatR.StreetCode.RelatedTerm
{
    public class CreateRelatedTermHandlerTest
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILoggerService> _loggerServiceMock;
        private readonly CreateRelatedTermHandler _handler;

        public CreateRelatedTermHandlerTest()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _loggerServiceMock = new Mock<ILoggerService>();

            _handler = new CreateRelatedTermHandler(_repositoryWrapperMock.Object, _mapperMock.Object, _loggerServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_WhenMappedEntityIsNull()
        {
            var request = new CreateRelatedTermCommand(new RelatedTermDTO());
            _mapperMock.Setup(m => m.Map<RelatedTermDTO>(It.IsAny<RelatedTermDTO>())).Returns((RelatedTermDTO)null);

            var result = await _handler.Handle(request, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Errors.First().Message.Should().Be("Cannot create new related word for a term!");

            _loggerServiceMock.Verify(x => x.LogError(request, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_WhenMappedEntityExists()
        {
            var list = new List<Entity> { new Entity { Id = 0, Word = "test", TermId = 1 } };
            var relatedTerm = new RelatedTermDTO { Id = 0, Word = "test", TermId = 1 };
            var mappedEntity = new Entity { Id = 0, Word = "test", TermId = 1 };
            var request = new CreateRelatedTermCommand(relatedTerm);

            _mapperMock.Setup(m => m.Map<Entity>(request.RelatedTerm)).Returns(mappedEntity);

            _repositoryWrapperMock.Setup(r => r.RelatedTermRepository.GetAllAsync(
                It.IsAny<Expression<Func<Entity, bool>>>(),
                It.IsAny<Func<IQueryable<Entity>, IIncludableQueryable<Entity, object>>>()))
                .ReturnsAsync(list);

            var result = await _handler.Handle(request, CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal("Слово з цим визначенням уже існує", result.Errors.First().Message);
            _loggerServiceMock.Verify(l => l.LogError(request, "Слово з цим визначенням уже існує"), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldFail_WhenSaveChangesFails()
        {
            var relatedTerm = new RelatedTermDTO() { Id = 0, TermId = 1 };
            var entity = new Entity();
            var request = new CreateRelatedTermCommand(relatedTerm);

            _mapperMock.Setup(m => m.Map<Entity>(relatedTerm)).Returns(entity);
            _repositoryWrapperMock.Setup(r => r.RelatedTermRepository.GetAllAsync(
                It.IsAny<Expression<Func<Entity, bool>>>(),
                It.IsAny<Func<IQueryable<Entity>, IIncludableQueryable<Entity, object>>>()))
                .ReturnsAsync(new List<Entity>());

            _repositoryWrapperMock.Setup(r => r.RelatedTermRepository.Create(entity)).Returns(entity);
            _repositoryWrapperMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0);

            var result = await _handler.Handle(request, CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal("Cannot save changes in the database after related word creation!", result.Errors.First().Message);
            _loggerServiceMock.Verify(l => l.LogError(request, "Cannot save changes in the database after related word creation!"), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldFail_WhenMappingDtoFails()
        {
            var relatedTerm = new RelatedTermDTO { Id = 1, TermId = 1 };
            var entity = new Entity();
            var request = new CreateRelatedTermCommand(relatedTerm);

            _mapperMock.Setup(m => m.Map<Entity>(relatedTerm)).Returns(entity);
            _repositoryWrapperMock.Setup(r => r.RelatedTermRepository.GetAllAsync(
                It.IsAny<Expression<Func<Entity, bool>>>(),
                It.IsAny<Func<IQueryable<Entity>, IIncludableQueryable<Entity, object>>>()))
                .ReturnsAsync(new List<Entity>());

            _repositoryWrapperMock.Setup(r => r.RelatedTermRepository.Create(entity)).Returns(entity);
            _repositoryWrapperMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
            _mapperMock.Setup(m => m.Map<RelatedTermDTO>(entity)).Returns((RelatedTermDTO)null);

            var result = await _handler.Handle(request, CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal("Cannot map entity!", result.Errors.First().Message);
            _loggerServiceMock.Verify(l => l.LogError(request, "Cannot map entity!"), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldSucceed_WhenValidInput()
        {
            var relatedTerm = new RelatedTermDTO { Id = 1, TermId = 1 };
            var entity = new Entity();
            var request = new CreateRelatedTermCommand(relatedTerm);

            _mapperMock.Setup(m => m.Map<Entity>(relatedTerm)).Returns(entity);
            _repositoryWrapperMock.Setup(r => r.RelatedTermRepository.GetAllAsync(
                It.IsAny<Expression<Func<Entity, bool>>>(),
                It.IsAny<Func<IQueryable<Entity>, IIncludableQueryable<Entity, object>>>()))
                .ReturnsAsync(new List<Entity>());

            _repositoryWrapperMock.Setup(r => r.RelatedTermRepository.Create(entity)).Returns(entity);
            _repositoryWrapperMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
            _mapperMock.Setup(m => m.Map<RelatedTermDTO>(entity)).Returns(relatedTerm);

            var result = await _handler.Handle(request, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(relatedTerm, result.Value);
        }
    }
}
