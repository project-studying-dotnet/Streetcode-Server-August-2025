using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Moq;
using Streetcode.BLL.DTO.Sources;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Sources.SourceLinkCategory.Create;
using Streetcode.BLL.MediatR.Sources.SourceLinkCategory.Update;
using Streetcode.BLL.MediatR.Sources.StreetcodeCategoryContent.Update;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.Sources.SourceLinkCategory.Update
{
    public class UpdateSourceLinkCategoryHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILoggerService> _mockLogger;
        private readonly UpdateSourceLinkCategoryHandler _handler;
        public UpdateSourceLinkCategoryHandlerTests()
        {
            _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILoggerService>();
            _handler = new UpdateSourceLinkCategoryHandler(
                _mockMapper.Object,
                _mockLogger.Object,
                _mockRepositoryWrapper.Object);
        }

        [Fact]
        public async Task HandleShouldReturnOkResult_WhenUpdateIsSuccess()
        {
            var updateDto = new SourceLinkCategoryUpdateDTO
            {
                Id = 1,
                Title = "Updated text",
                ImageId = 2
            };

            var existingEntity = new DAL.Entities.Sources.SourceLinkCategory
            {
                Id = 1,
                Title = "Old text",
                ImageId = 1
            };

            var expectedDto = new SourceLinkCategoryDTO
            {
                Id = 1,
                Title = "Updated text",
                ImageId = 2
            };

            _mockRepositoryWrapper.SetupSequence(r => r.SourceCategoryRepository
                .GetFirstOrDefaultAsync(It.IsAny<Expression<Func<DAL.Entities.Sources.SourceLinkCategory, bool>>>(), null))
                .ReturnsAsync(existingEntity) // перший виклик (по Id)
                .ReturnsAsync((DAL.Entities.Sources.SourceLinkCategory)null); // другий виклик (перевірка дублікатів)

            _mockMapper.Setup(m => m.Map(updateDto, existingEntity));

            _mockRepositoryWrapper.Setup(r => r.SourceCategoryRepository.Update(existingEntity));

            _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

            _mockMapper.Setup(m => m.Map<SourceLinkCategoryDTO>(existingEntity)).Returns(expectedDto);

            var result = await _handler.Handle(new UpdateSourceLinkCategoryCommand(updateDto), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            _mockRepositoryWrapper.Verify(
                r => r.SourceCategoryRepository
                .GetFirstOrDefaultAsync(It.IsAny<Expression<Func<DAL.Entities.Sources.SourceLinkCategory, bool>>>(), null),
                Times.Exactly(2));
            _mockMapper.Verify(m => m.Map(updateDto, existingEntity), Times.Once);
            _mockRepositoryWrapper.Verify(r => r.SourceCategoryRepository.Update(existingEntity), Times.Once);
            _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Once);
            _mockMapper.Verify(m => m.Map<SourceLinkCategoryDTO>(existingEntity), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_WhenEntityNotFound()
        {
            var updateDto = new SourceLinkCategoryUpdateDTO
            {
                Id = 1,
                Title = "Updated text",
                ImageId = 2
            };

            var existingEntity = new DAL.Entities.Sources.SourceLinkCategory
            {
                Id = 1,
                Title = "Old text",
                ImageId = 1
            };

            _mockRepositoryWrapper.Setup(r => r.SourceCategoryRepository
            .GetFirstOrDefaultAsync(It.IsAny<Expression<Func<DAL.Entities.Sources.SourceLinkCategory, bool>>>(), null))
                .ReturnsAsync((DAL.Entities.Sources.SourceLinkCategory)null);

            var result = await _handler.Handle(new UpdateSourceLinkCategoryCommand(updateDto), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle(e =>
                e.Message == "Category not found.");

            _mockRepositoryWrapper.Verify(
                r => r.SourceCategoryRepository
                .GetFirstOrDefaultAsync(It.IsAny<Expression<Func<DAL.Entities.Sources.SourceLinkCategory, bool>>>(), null),
                Times.Once);
            _mockMapper.Verify(m => m.Map(updateDto, existingEntity), Times.Never);
            _mockRepositoryWrapper.Verify(r => r.SourceCategoryRepository.Update(existingEntity), Times.Never);
            _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Never);
            _mockMapper.Verify(m => m.Map<SourceLinkCategoryDTO>(existingEntity), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_WhenFoundDuplicates()
        {
            var updateDto = new SourceLinkCategoryUpdateDTO
            {
                Id = 1,
                Title = "Updated text",
                ImageId = 2
            };

            var existingEntity = new DAL.Entities.Sources.SourceLinkCategory
            {
                Id = 1,
                Title = "Old text",
                ImageId = 1
            };

            _mockRepositoryWrapper.SetupSequence(r => r.SourceCategoryRepository
                .GetFirstOrDefaultAsync(It.IsAny<Expression<Func<DAL.Entities.Sources.SourceLinkCategory, bool>>>(), null))
                .ReturnsAsync(existingEntity) // перший виклик (по Id)
                .ReturnsAsync(existingEntity);

            _mockMapper.Setup(m => m.Map(updateDto, existingEntity));

            var result = await _handler.Handle(new UpdateSourceLinkCategoryCommand(updateDto), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle(e =>
                e.Message == "Category with the same title or image already exists.");
            _mockRepositoryWrapper.Verify(
                r => r.SourceCategoryRepository
                .GetFirstOrDefaultAsync(It.IsAny<Expression<Func<DAL.Entities.Sources.SourceLinkCategory, bool>>>(), null),
                Times.Exactly(2));
            _mockMapper.Verify(m => m.Map(updateDto, existingEntity), Times.Once);
            _mockRepositoryWrapper.Verify(r => r.SourceCategoryRepository.Update(existingEntity), Times.Never);
            _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Never);
            _mockMapper.Verify(m => m.Map<SourceLinkCategoryDTO>(existingEntity), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailResult_WhileSave()
        {
            var updateDto = new SourceLinkCategoryUpdateDTO
            {
                Id = 1,
                Title = "Updated text",
                ImageId = 2
            };

            var existingEntity = new DAL.Entities.Sources.SourceLinkCategory
            {
                Id = 1,
                Title = "Old text",
                ImageId = 1
            };

            _mockRepositoryWrapper.SetupSequence(r => r.SourceCategoryRepository
                .GetFirstOrDefaultAsync(It.IsAny<Expression<Func<DAL.Entities.Sources.SourceLinkCategory, bool>>>(), null))
                .ReturnsAsync(existingEntity) // перший виклик (по Id)
                .ReturnsAsync((DAL.Entities.Sources.SourceLinkCategory)null); // другий виклик (перевірка дублікатів)

            _mockMapper.Setup(m => m.Map(updateDto, existingEntity));

            _mockRepositoryWrapper.Setup(r => r.SourceCategoryRepository.Update(existingEntity));

            _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0);

            var result = await _handler.Handle(new UpdateSourceLinkCategoryCommand(updateDto), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle(e =>
                e.Message == "Error while saving");
            _mockRepositoryWrapper.Verify(
                r => r.SourceCategoryRepository
                .GetFirstOrDefaultAsync(It.IsAny<Expression<Func<DAL.Entities.Sources.SourceLinkCategory, bool>>>(), null),
                Times.Exactly(2));
            _mockMapper.Verify(m => m.Map(updateDto, existingEntity), Times.Once);
            _mockRepositoryWrapper.Verify(r => r.SourceCategoryRepository.Update(existingEntity), Times.Once);
            _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Once);
            _mockMapper.Verify(m => m.Map<SourceLinkCategoryDTO>(existingEntity), Times.Never);
        }
    }
}
