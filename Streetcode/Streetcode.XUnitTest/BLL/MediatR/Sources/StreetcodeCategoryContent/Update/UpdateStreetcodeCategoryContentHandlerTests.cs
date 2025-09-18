using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Moq;
using Streetcode.BLL.DTO.Sources;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Sources.StreetcodeCategoryContent.Update;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.Sources.StreetcodeCategoryContent.Update
{
    public class UpdateStreetcodeCategoryContentHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILoggerService> _mockLogger;
        private readonly UpdateStreetcodeCategoryContentHandler _handler;

        public UpdateStreetcodeCategoryContentHandlerTests()
        {
            _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILoggerService>();
            _handler = new UpdateStreetcodeCategoryContentHandler(
                _mockMapper.Object,
                _mockLogger.Object,
                _mockRepositoryWrapper.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnOkResult_WhenUpdateIsSuccess()
        {
            var updateDto = new CategoryContentUpdateDTO
            {
                Id = 1,
                StreetcodeId = 1,
                SourceLinkCategoryId = 2,
                Text = "Updated text"
            };

            var existingEntity = new DAL.Entities.Sources.StreetcodeCategoryContent
            {
                Id = 1,
                StreetcodeId = 1,
                SourceLinkCategoryId = 1,
                Text = "Old text"
            };

            var updatedEntity = new DAL.Entities.Sources.StreetcodeCategoryContent
            {
                Id = 1,
                StreetcodeId = 1,
                SourceLinkCategoryId = 2,
                Text = "Updated text"
            };

            var expectedDto = new StreetcodeCategoryContentDTO
            {
                Id = 1,
                StreetcodeId = 1,
                SourceLinkCategoryId = 2,
                Text = "Updated text"
            };

            _mockRepositoryWrapper.SetupSequence(r => r.StreetcodeCategoryContentRepository
                .GetFirstOrDefaultAsync(It.IsAny<Expression<Func<DAL.Entities.Sources.StreetcodeCategoryContent, bool>>>(), null))
                .ReturnsAsync(existingEntity) // перший виклик (по Id)
                .ReturnsAsync((DAL.Entities.Sources.StreetcodeCategoryContent)null); // другий виклик (перевірка дублікатів)

            _mockMapper.Setup(m => m.Map(updateDto, existingEntity));

            _mockRepositoryWrapper.Setup(r => r.StreetcodeCategoryContentRepository.Update(existingEntity));

            _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

            _mockMapper.Setup(m => m.Map<StreetcodeCategoryContentDTO>(updatedEntity)).Returns(expectedDto);

            var result = await _handler.Handle(new UpdateStreetcodeCategoryContentCommand(updateDto), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            _mockRepositoryWrapper.Verify(
                r => r.StreetcodeCategoryContentRepository
                .GetFirstOrDefaultAsync(It.IsAny<Expression<Func<DAL.Entities.Sources.StreetcodeCategoryContent, bool>>>(), null),
                Times.Exactly(2));
            _mockMapper.Verify(m => m.Map(updateDto, existingEntity), Times.Once);
            _mockRepositoryWrapper.Verify(r => r.StreetcodeCategoryContentRepository.Update(existingEntity), Times.Once);
            _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Once);
            _mockMapper.Verify(m => m.Map<StreetcodeCategoryContentDTO>(existingEntity), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_WhenEntityNotFound()
        {
            var updateDto = new CategoryContentUpdateDTO
            {
                Id = 1,
                StreetcodeId = 1,
                SourceLinkCategoryId = 2,
                Text = "Updated text"
            };

            var existingEntity = new DAL.Entities.Sources.StreetcodeCategoryContent
            {
                Id = 1,
                StreetcodeId = 1,
                SourceLinkCategoryId = 1,
                Text = "Old text"
            };

            _mockRepositoryWrapper.Setup(r => r.StreetcodeCategoryContentRepository
            .GetFirstOrDefaultAsync(It.IsAny<Expression<Func<DAL.Entities.Sources.StreetcodeCategoryContent, bool>>>(), null))
                .ReturnsAsync((DAL.Entities.Sources.StreetcodeCategoryContent)null);

            var result = await _handler.Handle(new UpdateStreetcodeCategoryContentCommand(updateDto), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle(e =>
                e.Message == $"Cannot find any StreetcodeCategoryContent with corresponding id: {updateDto.Id}");
            _mockRepositoryWrapper.Verify(
                r => r.StreetcodeCategoryContentRepository
                .GetFirstOrDefaultAsync(It.IsAny<Expression<Func<DAL.Entities.Sources.StreetcodeCategoryContent, bool>>>(), null),
                Times.Once);

            _mockMapper.Verify(m => m.Map(updateDto, existingEntity), Times.Never);
            _mockRepositoryWrapper.Verify(r => r.StreetcodeCategoryContentRepository.Update(existingEntity), Times.Never);
            _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Never);
            _mockMapper.Verify(m => m.Map<StreetcodeCategoryContentDTO>(existingEntity), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_WhenFoundDuplicates()
        {
            var updateDto = new CategoryContentUpdateDTO
            {
                Id = 1,
                StreetcodeId = 1,
                SourceLinkCategoryId = 2,
                Text = "Updated text"
            };

            var existingEntity = new DAL.Entities.Sources.StreetcodeCategoryContent
            {
                Id = 1,
                StreetcodeId = 1,
                SourceLinkCategoryId = 1,
                Text = "Old text"
            };

            var updatedEntity = new DAL.Entities.Sources.StreetcodeCategoryContent
            {
                Id = 1,
                StreetcodeId = 1,
                SourceLinkCategoryId = 2,
                Text = "Updated text"
            };

            var expectedDto = new StreetcodeCategoryContentDTO
            {
                Id = 1,
                StreetcodeId = 1,
                SourceLinkCategoryId = 2,
                Text = "Updated text"
            };

            _mockRepositoryWrapper.SetupSequence(r => r.StreetcodeCategoryContentRepository
                .GetFirstOrDefaultAsync(It.IsAny<Expression<Func<DAL.Entities.Sources.StreetcodeCategoryContent, bool>>>(), null))
                .ReturnsAsync(existingEntity) // перший виклик (по Id)
                .ReturnsAsync(existingEntity);

            _mockMapper.Setup(m => m.Map(updateDto, existingEntity));

            var result = await _handler.Handle(new UpdateStreetcodeCategoryContentCommand(updateDto), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle(e =>
                e.Message == "CategoryContent with the same Streetcode and SourceLinkCategory already exists");
            _mockRepositoryWrapper.Verify(
                r => r.StreetcodeCategoryContentRepository
                .GetFirstOrDefaultAsync(It.IsAny<Expression<Func<DAL.Entities.Sources.StreetcodeCategoryContent, bool>>>(), null),
                Times.Exactly(2));
            _mockMapper.Verify(m => m.Map(updateDto, existingEntity), Times.Once);
            _mockRepositoryWrapper.Verify(r => r.StreetcodeCategoryContentRepository.Update(existingEntity), Times.Never);
            _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Never);
            _mockMapper.Verify(m => m.Map<StreetcodeCategoryContentDTO>(existingEntity), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailResult_WhileSave()
        {
            var updateDto = new CategoryContentUpdateDTO
            {
                Id = 1,
                StreetcodeId = 1,
                SourceLinkCategoryId = 2,
                Text = "Updated text"
            };

            var existingEntity = new DAL.Entities.Sources.StreetcodeCategoryContent
            {
                Id = 1,
                StreetcodeId = 1,
                SourceLinkCategoryId = 1,
                Text = "Old text"
            };

            _mockRepositoryWrapper.SetupSequence(r => r.StreetcodeCategoryContentRepository
                .GetFirstOrDefaultAsync(It.IsAny<Expression<Func<DAL.Entities.Sources.StreetcodeCategoryContent, bool>>>(), null))
                .ReturnsAsync(existingEntity) // перший виклик (по Id)
                .ReturnsAsync((DAL.Entities.Sources.StreetcodeCategoryContent)null); // другий виклик (перевірка дублікатів)

            _mockMapper.Setup(m => m.Map(updateDto, existingEntity));

            _mockRepositoryWrapper.Setup(r => r.StreetcodeCategoryContentRepository.Update(existingEntity));

            _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0);

            var result = await _handler.Handle(new UpdateStreetcodeCategoryContentCommand(updateDto), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle(e =>
                e.Message == "Failed to update a StreetcodeCategoryContent");
            _mockRepositoryWrapper.Verify(
               r => r.StreetcodeCategoryContentRepository
               .GetFirstOrDefaultAsync(It.IsAny<Expression<Func<DAL.Entities.Sources.StreetcodeCategoryContent, bool>>>(), null),
               Times.Exactly(2));
            _mockMapper.Verify(m => m.Map(updateDto, existingEntity), Times.Once);
            _mockRepositoryWrapper.Verify(r => r.StreetcodeCategoryContentRepository.Update(existingEntity), Times.Once);
            _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Once);
            _mockMapper.Verify(m => m.Map<StreetcodeCategoryContentDTO>(existingEntity), Times.Never);
        }
    }
}
