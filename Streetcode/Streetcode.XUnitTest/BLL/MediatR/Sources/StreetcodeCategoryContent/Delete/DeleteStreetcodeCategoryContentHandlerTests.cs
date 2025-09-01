using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Sources;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Sources.StreetcodeCategoryContent.Create;
using Streetcode.BLL.MediatR.Sources.StreetcodeCategoryContent.Delete;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.Sources.StreetcodeCategoryContent.Delete
{
    public class DeleteStreetcodeCategoryContentHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILoggerService> _mockLogger;
        private readonly DeleteStreetcodeCategoryContentHandler _handler;
        public DeleteStreetcodeCategoryContentHandlerTests()
        {
            _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILoggerService>();
            _handler = new DeleteStreetcodeCategoryContentHandler(
                _mockLogger.Object,
                _mockMapper.Object,
                _mockRepositoryWrapper.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnOkResult_WhenDeleteSuccess()
        {
            var entity = new DAL.Entities.Sources.StreetcodeCategoryContent
            {
                Id = 1,
                StreetcodeId = 1,
                SourceLinkCategoryId = 2
            };
            var dto = new StreetcodeCategoryContentDTO
            {
                Id = 1,
                StreetcodeId = 1,
                SourceLinkCategoryId = 2
            };

            _mockRepositoryWrapper.Setup(r => r.StreetcodeCategoryContentRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<DAL.Entities.Sources.StreetcodeCategoryContent, bool>>>(),
                It.IsAny<Func<IQueryable<DAL.Entities.Sources.StreetcodeCategoryContent>, IIncludableQueryable<DAL.Entities.Sources.StreetcodeCategoryContent, object>>>()))
                .ReturnsAsync(entity);

            _mockRepositoryWrapper.Setup(r => r.StreetcodeCategoryContentRepository.Delete(entity));

            _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

            _mockMapper.Setup(m => m.Map<StreetcodeCategoryContentDTO>(entity));

            var result = await _handler.Handle(new DeleteStreetcodeCategoryContentCommand(entity.Id), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            _mockRepositoryWrapper.Verify(r => r.StreetcodeCategoryContentRepository.Delete(entity), Times.Once);
            _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Once);
            _mockMapper.Verify(m => m.Map<StreetcodeCategoryContentDTO>(entity), Times.Once);
            _mockRepositoryWrapper.Verify(
                r => r.StreetcodeCategoryContentRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<DAL.Entities.Sources.StreetcodeCategoryContent, bool>>>(),
                    It.IsAny<Func<IQueryable<DAL.Entities.Sources.StreetcodeCategoryContent>, IIncludableQueryable<DAL.Entities.Sources.StreetcodeCategoryContent, object>>>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailResult_WhenEntityNotExist()
        {
            var entity = new DAL.Entities.Sources.StreetcodeCategoryContent
            {
                Id = 1,
                StreetcodeId = 1,
                SourceLinkCategoryId = 2
            };
            var dto = new StreetcodeCategoryContentDTO
            {
                Id = 1,
                StreetcodeId = 1,
                SourceLinkCategoryId = 2
            };

            _mockRepositoryWrapper.Setup(r => r.StreetcodeCategoryContentRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<DAL.Entities.Sources.StreetcodeCategoryContent, bool>>>(),
                It.IsAny<Func<IQueryable<DAL.Entities.Sources.StreetcodeCategoryContent>, IIncludableQueryable<DAL.Entities.Sources.StreetcodeCategoryContent, object>>>()))
                .ReturnsAsync((DAL.Entities.Sources.StreetcodeCategoryContent)null);

            var result = await _handler.Handle(new DeleteStreetcodeCategoryContentCommand(entity.Id), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle(e =>
            e.Message == "StreetcodeCategoryContent don`t exist.");
            _mockRepositoryWrapper.Verify(r => r.StreetcodeCategoryContentRepository.Delete(entity), Times.Never);
            _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Never);
            _mockMapper.Verify(m => m.Map<StreetcodeCategoryContentDTO>(entity), Times.Never);
            _mockRepositoryWrapper.Verify(
                r => r.StreetcodeCategoryContentRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<DAL.Entities.Sources.StreetcodeCategoryContent, bool>>>(),
                    It.IsAny<Func<IQueryable<DAL.Entities.Sources.StreetcodeCategoryContent>, IIncludableQueryable<DAL.Entities.Sources.StreetcodeCategoryContent, object>>>()), Times.Once);
        }

        [Fact]
        public async Task HandleShouldReturnFailResult_WhileSaving()
        {
            var entity = new DAL.Entities.Sources.StreetcodeCategoryContent
            {
                Id = 1,
                StreetcodeId = 1,
                SourceLinkCategoryId = 2
            };
            var dto = new StreetcodeCategoryContentDTO
            {
                Id = 1,
                StreetcodeId = 1,
                SourceLinkCategoryId = 2
            };

            _mockRepositoryWrapper.Setup(r => r.StreetcodeCategoryContentRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<DAL.Entities.Sources.StreetcodeCategoryContent, bool>>>(),
                It.IsAny<Func<IQueryable<DAL.Entities.Sources.StreetcodeCategoryContent>, IIncludableQueryable<DAL.Entities.Sources.StreetcodeCategoryContent, object>>>()))
                .ReturnsAsync(entity);

            _mockRepositoryWrapper.Setup(r => r.StreetcodeCategoryContentRepository.Delete(entity));

            _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0);

            var result = await _handler.Handle(new DeleteStreetcodeCategoryContentCommand(entity.Id), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle(e =>
            e.Message == "Failed to delete streetcodeCategoryContent.");
            _mockRepositoryWrapper.Verify(r => r.StreetcodeCategoryContentRepository.Delete(entity), Times.Once);
            _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Once);
            _mockMapper.Verify(m => m.Map<StreetcodeCategoryContentDTO>(entity), Times.Never);
            _mockRepositoryWrapper.Verify(
                r => r.StreetcodeCategoryContentRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<DAL.Entities.Sources.StreetcodeCategoryContent, bool>>>(),
                    It.IsAny<Func<IQueryable<DAL.Entities.Sources.StreetcodeCategoryContent>, IIncludableQueryable<DAL.Entities.Sources.StreetcodeCategoryContent, object>>>()), Times.Once);
        }
    }
}
