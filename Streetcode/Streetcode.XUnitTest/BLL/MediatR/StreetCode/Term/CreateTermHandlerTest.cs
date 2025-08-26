using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Streetcode.TextContent;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.RelatedTerm.Create;
using Streetcode.BLL.MediatR.Streetcode.Term.Create;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

using Entity = Streetcode.DAL.Entities.Streetcode.TextContent.Term;

namespace Streetcode.XUnitTest.BLL.MediatR.StreetCode.Term
{
    public class CreateTermHandlerTest
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILoggerService> _loggerServiceMock;
        private readonly CreateTermHandler _handler;

        public CreateTermHandlerTest()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _loggerServiceMock = new Mock<ILoggerService>();
            _handler = new CreateTermHandler(_repositoryWrapperMock.Object, _mapperMock.Object, _loggerServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldFail_WhenEntityIsNull()
        {
            var request = new CreateTermCommand(new TermDTO());
            _mapperMock.Setup(m => m.Map<TermDTO>(It.IsAny<TermDTO>())).Returns((TermDTO)null);

            var result = await _handler.Handle(request, CancellationToken.None);

            Assert.True(result.IsFailed);
            Assert.Equal("cannot create a new term", result.Errors.First().Message);
        }

        [Fact]
        public async Task Handle_ShouldFail_WhenEntityExists()
        {
            var list = new List<Entity> { new Entity { Id = 1, Title = "test" } };
            var term = new TermDTO { Id = 1, Title = "test" };
            var mappedEntity = new Entity { Id = 1, Title = "test" };
            var request = new CreateTermCommand(term);

            _mapperMock.Setup(m => m.Map<Entity>(request.Term)).Returns(mappedEntity);

            _repositoryWrapperMock.Setup(r => r.TermRepository.GetAllAsync(
                It.IsAny<Expression<Func<Entity, bool>>>(),
                It.IsAny<Func<IQueryable<Entity>, IIncludableQueryable<Entity, object>>>()))
                .ReturnsAsync(list);

            var result = await _handler.Handle(request, CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal("requested term already exists", result.Errors.First().Message);
        }

        [Fact]
        public async Task Handle_ShouldFail_WhenSaveChangesFail()
        {
            var term = new TermDTO() { Id = 0, Title = "test" };
            var entity = new Entity() { Id = 0, Title = "test" };
            var request = new CreateTermCommand(term);

            _mapperMock.Setup(m => m.Map<Entity>(term)).Returns(entity);
            _repositoryWrapperMock.Setup(r => r.TermRepository.GetAllAsync(
                It.IsAny<Expression<Func<Entity, bool>>>(),
                It.IsAny<Func<IQueryable<Entity>, IIncludableQueryable<Entity, object>>>()))
                .ReturnsAsync(new List<Entity>());

            _repositoryWrapperMock.Setup(r => r.TermRepository.Create(entity)).Returns(entity);
            _repositoryWrapperMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0);

            var result = await _handler.Handle(request, CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal("Cannot save changes in database", result.Errors.First().Message);
        }

        [Fact]
        public async Task Handle_ShouldFail_WhenMappingDtoFails()
        {
            var term = new TermDTO { Id = 1, Title = "test" };
            var entity = new Entity() { Id = 1, Title = "test" };
            var request = new CreateTermCommand(term);

            _mapperMock.Setup(m => m.Map<Entity>(term)).Returns(entity);
            _repositoryWrapperMock.Setup(r => r.TermRepository.GetAllAsync(
                It.IsAny<Expression<Func<Entity, bool>>>(),
                It.IsAny<Func<IQueryable<Entity>, IIncludableQueryable<Entity, object>>>()))
                .ReturnsAsync(new List<Entity>());

            _repositoryWrapperMock.Setup(r => r.TermRepository.Create(entity)).Returns(entity);
            _repositoryWrapperMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
            _mapperMock.Setup(m => m.Map<TermDTO>(entity)).Returns((TermDTO)null);

            var result = await _handler.Handle(request, CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal("cannot map entity", result.Errors.First().Message);
        }

        [Fact]
        public async Task Handle_ShouldSucceed_WhenValidInput()
        {
            var term = new TermDTO { Id = 1, Title = "test" };
            var entity = new Entity() { Id = 1, Title = "test" };
            var request = new CreateTermCommand(term);

            _mapperMock.Setup(m => m.Map<Entity>(term)).Returns(entity);
            _repositoryWrapperMock.Setup(r => r.TermRepository.GetAllAsync(
                It.IsAny<Expression<Func<Entity, bool>>>(),
                It.IsAny<Func<IQueryable<Entity>, IIncludableQueryable<Entity, object>>>()))
                .ReturnsAsync(new List<Entity>());

            _repositoryWrapperMock.Setup(r => r.TermRepository.Create(entity)).Returns(entity);
            _repositoryWrapperMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
            _mapperMock.Setup(m => m.Map<TermDTO>(entity)).Returns(term);

            var result = await _handler.Handle(request, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(term, result.Value);
        }
    }
}
