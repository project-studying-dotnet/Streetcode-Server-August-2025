using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using FluentResults;
using Moq;
using Streetcode.BLL.DTO.Sources;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Sources.SourceLinkCategory.Create;
using Streetcode.BLL.MediatR.Sources.StreetcodeCategoryContent.Create;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.Sources.SourceLinkCategory.Create
{
    public class CreateSourceLinkCategoryHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILoggerService> _mockLogger;
        private readonly CreateSourceLinkCategoryHandler _handler;
        public CreateSourceLinkCategoryHandlerTests()
        {
            _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILoggerService>();
            _handler = new CreateSourceLinkCategoryHandler(
                _mockMapper.Object,
                _mockLogger.Object,
                _mockRepositoryWrapper.Object);
        }

        [Fact]
        public async Task HandleShouldReturnOkResult_WhenCreateIsSuccess()
        {
            var createDto = new SourceLinkCategoryCreateDTO
            {
                Title = "Title",
                ImageId = 1
            };
            var entity = new DAL.Entities.Sources.SourceLinkCategory
            {
                Title = "Title",
                ImageId = 1
            };
            var returnsDto = new SourceLinkCategoryDTO
            {
                Title = "Title",
                ImageId = 1
            };
            _mockMapper.Setup(m => m.Map<DAL.Entities.Sources.SourceLinkCategory>(createDto))
                .Returns(entity);

            _mockRepositoryWrapper.Setup(r => r.SourceCategoryRepository
            .GetFirstOrDefaultAsync(
               It.IsAny<Expression<Func<DAL.Entities.Sources.SourceLinkCategory, bool>>>(), null))
                .ReturnsAsync((DAL.Entities.Sources.SourceLinkCategory)null);

            _mockRepositoryWrapper.Setup(r => r.SourceCategoryRepository.CreateAsync(entity))
                .ReturnsAsync(entity);

            _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

            _mockMapper.Setup(m => m.Map<SourceLinkCategoryDTO>(entity))
                .Returns(returnsDto);

            var result = await _handler.Handle(new CreateSourceLinkCategoryCommand(createDto), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(returnsDto);
            result.Value.Should().Be(returnsDto);

            _mockRepositoryWrapper.Verify(
                r => r.SourceCategoryRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<DAL.Entities.Sources.SourceLinkCategory, bool>>>(), null), Times.Once);
            _mockMapper.Verify(m => m.Map<DAL.Entities.Sources.SourceLinkCategory>(createDto), Times.Once);
            _mockRepositoryWrapper.Verify(r => r.SourceCategoryRepository.CreateAsync(entity), Times.Once);
            _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Once);
            _mockMapper.Verify(m => m.Map<SourceLinkCategoryDTO>(entity), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_When_SourceLinkCategoryAlreadyExist()
        {
            var createDto = new SourceLinkCategoryCreateDTO
            {
                Title = "Title",
                ImageId = 1
            };
            var entity = new DAL.Entities.Sources.SourceLinkCategory
            {
                Title = "Title",
                ImageId = 1
            };
            var returnsDto = new SourceLinkCategoryDTO
            {
                Title = "Title",
                ImageId = 1
            };
            _mockMapper.Setup(m => m.Map<DAL.Entities.Sources.SourceLinkCategory>(createDto))
                .Returns(entity);

            _mockRepositoryWrapper.Setup(r => r.SourceCategoryRepository
            .GetFirstOrDefaultAsync(
               It.IsAny<Expression<Func<DAL.Entities.Sources.SourceLinkCategory, bool>>>(), null))
                .ReturnsAsync(entity);

            var result = await _handler.Handle(new CreateSourceLinkCategoryCommand(createDto), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle(e =>
            e.Message == "Category with the same title or image already exists.");

            _mockRepositoryWrapper.Verify(
                r => r.SourceCategoryRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<DAL.Entities.Sources.SourceLinkCategory, bool>>>(), null), Times.Once);
            _mockMapper.Verify(m => m.Map<DAL.Entities.Sources.SourceLinkCategory>(createDto), Times.Once);
            _mockRepositoryWrapper.Verify(r => r.SourceCategoryRepository.CreateAsync(entity), Times.Never);
            _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Never);
            _mockMapper.Verify(m => m.Map<SourceLinkCategoryDTO>(entity), Times.Never);
        }

        [Fact]
        public async Task Handle_SHouldReturnFail_WhileCreating()
        {
            var createDto = new SourceLinkCategoryCreateDTO
            {
                Title = "Title",
                ImageId = 1
            };
            var entity = new DAL.Entities.Sources.SourceLinkCategory
            {
                Title = "Title",
                ImageId = 1
            };
            var returnsDto = new SourceLinkCategoryDTO
            {
                Title = "Title",
                ImageId = 1
            };
            _mockMapper.Setup(m => m.Map<DAL.Entities.Sources.SourceLinkCategory>(createDto))
                .Returns(entity);

            _mockRepositoryWrapper.Setup(r => r.SourceCategoryRepository
            .GetFirstOrDefaultAsync(
               It.IsAny<Expression<Func<DAL.Entities.Sources.SourceLinkCategory, bool>>>(), null))
                .ReturnsAsync((DAL.Entities.Sources.SourceLinkCategory)null);

            _mockRepositoryWrapper.Setup(r => r.SourceCategoryRepository.CreateAsync(entity))
                .ReturnsAsync(entity);

            _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0);

            var result = await _handler.Handle(new CreateSourceLinkCategoryCommand(createDto), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle(e =>
                e.Message == "Failed to create category");

            _mockRepositoryWrapper.Verify(
                r => r.SourceCategoryRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<DAL.Entities.Sources.SourceLinkCategory, bool>>>(), null), Times.Once);
            _mockMapper.Verify(m => m.Map<DAL.Entities.Sources.SourceLinkCategory>(createDto), Times.Once);
            _mockRepositoryWrapper.Verify(r => r.SourceCategoryRepository.CreateAsync(entity), Times.Once);
            _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Once);
            _mockMapper.Verify(m => m.Map<SourceLinkCategoryDTO>(entity), Times.Never);
        }
    }
}
