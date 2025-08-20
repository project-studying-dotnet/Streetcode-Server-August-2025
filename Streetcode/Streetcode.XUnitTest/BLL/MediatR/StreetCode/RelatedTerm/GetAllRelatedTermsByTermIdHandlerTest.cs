using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using MimeKit.Cryptography;
using Moq;
using Streetcode.BLL.DTO.Streetcode.TextContent;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.RelatedTerm.Delete;
using Streetcode.BLL.MediatR.Streetcode.RelatedTerm.GetAllByTermId;
using Streetcode.DAL.Entities.Streetcode.TextContent;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Entity = Streetcode.DAL.Entities.Streetcode.TextContent.RelatedTerm;

namespace Streetcode.XUnitTest.BLL.MediatR.StreetCode.RelatedTerm
{
    public class GetAllRelatedTermsByTermIdHandlerTest
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILoggerService> _loggerServiceMock;
        private readonly GetAllRelatedTermsByTermIdHandler _handler;

        public GetAllRelatedTermsByTermIdHandlerTest()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _loggerServiceMock = new Mock<ILoggerService>();

            _handler = new GetAllRelatedTermsByTermIdHandler(_mapperMock.Object, _repositoryWrapperMock.Object, _loggerServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldSucceed_WhenRelatedTermExists()
        {
            var request = new GetAllRelatedTermsByTermIdQuery(1);
            var entities = new List<Entity> { new Entity { Id = 1, TermId = 1 } };
            var relatedTermsDTOs = new List<RelatedTermDTO> { new RelatedTermDTO { Id = 1, TermId = 1 } };

            _repositoryWrapperMock.Setup(r => r.RelatedTermRepository.GetAllAsync(
            It.IsAny<Expression<Func<Entity, bool>>>(),
            It.IsAny<Func<IQueryable<Entity>, IIncludableQueryable<Entity, object>>>()))
            .ReturnsAsync(entities);

            _mapperMock.Setup(m => m.Map<IEnumerable<RelatedTermDTO>>(entities)).Returns(relatedTermsDTOs);

            var result = await _handler.Handle(request, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(result.Value, relatedTermsDTOs);
        }

        [Fact]
        public async Task Handle_ShouldFail_WhenRelatedTermIsNull()
        {
            var request = new GetAllRelatedTermsByTermIdQuery(1);

            _repositoryWrapperMock.Setup(r => r.RelatedTermRepository.GetAllAsync(
            It.IsAny<Expression<Func<Entity, bool>>>(),
            It.IsAny<Func<IQueryable<Entity>, IIncludableQueryable<Entity, object>>>()))
            .ReturnsAsync((IEnumerable<Entity>)null);

            var result = await _handler.Handle(request, CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(result.Errors.FirstOrDefault().Message, "Cannot get words by term id");
            _loggerServiceMock.Verify(l => l.LogError(request, "Cannot get words by term id"), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldFail_WhenMappingReturnsNull()
        {
            var request = new GetAllRelatedTermsByTermIdQuery(1);
            var entities = new List<Entity> { new Entity { Id = 1, TermId = 1 } };

            _repositoryWrapperMock.Setup(r => r.RelatedTermRepository.GetAllAsync(
            It.IsAny<Expression<Func<Entity, bool>>>(),
            It.IsAny<Func<IQueryable<Entity>, IIncludableQueryable<Entity, object>>>()))
            .ReturnsAsync(entities);

            _mapperMock.Setup(m => m.Map<IEnumerable<RelatedTermDTO>>(entities))
            .Returns((IEnumerable<RelatedTermDTO>)null);

            var result = await _handler.Handle(request, CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(result.Errors.FirstOrDefault().Message, "Cannot create DTOs for related words!");
            _loggerServiceMock.Verify(l => l.LogError(request, "Cannot create DTOs for related words!"), Times.Once);
        }
    }
}
