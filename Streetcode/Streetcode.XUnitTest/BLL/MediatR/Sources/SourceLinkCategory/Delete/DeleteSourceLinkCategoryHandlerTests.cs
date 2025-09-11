using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Sources;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Sources.SourceLinkCategory.Delete;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.Sources.SourceLinkCategory.Delete
{
    public class DeleteSourceLinkCategoryHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILoggerService> _mockLogger;
        private readonly DeleteSourceLinkCategoryHandler _handler;
        public DeleteSourceLinkCategoryHandlerTests()
        {
            _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILoggerService>();
            _handler = new DeleteSourceLinkCategoryHandler(
                _mockLogger.Object,
                _mockMapper.Object,
                _mockRepositoryWrapper.Object);
        }

        [Fact]
        public async Task Hadle_ShoudlReturnOkResult_WhenDeleteIsSuccess()
        {
            var entity = new DAL.Entities.Sources.SourceLinkCategory
            {
                Id = 1,
                Title = "Title",
                ImageId = 1
            };
            var dto = new SourceLinkCategoryDTO
            {
                Id = 1,
                Title = "Title",
                ImageId = 1
            };

            _mockRepositoryWrapper.Setup(r => r.SourceCategoryRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<DAL.Entities.Sources.SourceLinkCategory, bool>>>(),
                It.IsAny<Func<IQueryable<DAL.Entities.Sources.SourceLinkCategory>, IIncludableQueryable<DAL.Entities.Sources.SourceLinkCategory, object>>>()))
                .ReturnsAsync(entity);

            _mockRepositoryWrapper.Setup(r => r.SourceCategoryRepository.Delete(entity));

            _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

            _mockMapper.Setup(m => m.Map<SourceLinkCategoryDTO>(entity));

            var result = await _handler.Handle(new DeleteSourceLinkCategoryCommand(entity.Id), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            _mockRepositoryWrapper.Verify(r => r.SourceCategoryRepository.Delete(entity), Times.Once);
            _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Once);
            _mockMapper.Verify(m => m.Map<SourceLinkCategoryDTO>(entity), Times.Once);
            _mockRepositoryWrapper.Verify(
                r => r.SourceCategoryRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<DAL.Entities.Sources.SourceLinkCategory, bool>>>(),
                    It.IsAny<Func<IQueryable<DAL.Entities.Sources.SourceLinkCategory>, IIncludableQueryable<DAL.Entities.Sources.SourceLinkCategory, object>>>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailResult_WhenEntityNotFound()
        {
            var entity = new DAL.Entities.Sources.SourceLinkCategory
            {
                Id = 1,
                Title = "Title",
                ImageId = 1
            };
            var dto = new SourceLinkCategoryDTO
            {
                Id = 1,
                Title = "Title",
                ImageId = 1
            };

            _mockRepositoryWrapper.Setup(r => r.SourceCategoryRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<DAL.Entities.Sources.SourceLinkCategory, bool>>>(),
                It.IsAny<Func<IQueryable<DAL.Entities.Sources.SourceLinkCategory>, IIncludableQueryable<DAL.Entities.Sources.SourceLinkCategory, object>>>()))
                .ReturnsAsync((DAL.Entities.Sources.SourceLinkCategory)null);

            string errorMsg = Errors_Common.NotFoundById.FormatWith("SourceLinkCategory", entity.Id);

            var result = await _handler.Handle(new DeleteSourceLinkCategoryCommand(entity.Id), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.Message == errorMsg);
            _mockRepositoryWrapper.Verify(r => r.SourceCategoryRepository.Delete(entity), Times.Never);
            _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Never);
            _mockMapper.Verify(m => m.Map<SourceLinkCategoryDTO>(entity), Times.Never);
            _mockRepositoryWrapper.Verify(
                r => r.SourceCategoryRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<DAL.Entities.Sources.SourceLinkCategory, bool>>>(),
                    It.IsAny<Func<IQueryable<DAL.Entities.Sources.SourceLinkCategory>, IIncludableQueryable<DAL.Entities.Sources.SourceLinkCategory, object>>>()), Times.Once);
        }

        [Fact]
        public async Task HandleShouldReturnFailResult_WhileSaving()
        {
            var entity = new DAL.Entities.Sources.SourceLinkCategory
            {
                Id = 1,
                Title = "Title",
                ImageId = 1
            };
            var dto = new SourceLinkCategoryDTO
            {
                Id = 1,
                Title = "Title",
                ImageId = 1
            };
            _mockRepositoryWrapper.Setup(r => r.SourceCategoryRepository.GetFirstOrDefaultAsync(
               It.IsAny<Expression<Func<DAL.Entities.Sources.SourceLinkCategory, bool>>>(),
               It.IsAny<Func<IQueryable<DAL.Entities.Sources.SourceLinkCategory>, IIncludableQueryable<DAL.Entities.Sources.SourceLinkCategory, object>>>()))
               .ReturnsAsync(entity);

            _mockRepositoryWrapper.Setup(r => r.SourceCategoryRepository.Delete(entity));

            _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0);

            string errorMsg = Errors_Common.FailedToDelete.FormatWith("SourceLinkCategory");

            var result = await _handler.Handle(new DeleteSourceLinkCategoryCommand(entity.Id), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.Message == errorMsg);
            _mockRepositoryWrapper.Verify(r => r.SourceCategoryRepository.Delete(entity), Times.Once);
            _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Once);
            _mockMapper.Verify(m => m.Map<SourceLinkCategoryDTO>(entity), Times.Never);
            _mockRepositoryWrapper.Verify(
                r => r.SourceCategoryRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<DAL.Entities.Sources.SourceLinkCategory, bool>>>(),
                    It.IsAny<Func<IQueryable<DAL.Entities.Sources.SourceLinkCategory>, IIncludableQueryable<DAL.Entities.Sources.SourceLinkCategory, object>>>()), Times.Once);
        }
    }
}
