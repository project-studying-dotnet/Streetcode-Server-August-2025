using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Moq;
using Streetcode.BLL.DTO.Sources;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Sources.StreetcodeCategoryContent.Create;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.Sources.StreetcodeCategoryContent.Create
{
    public class CreateStreetcodeCategoryContentHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILoggerService> _mockLogger;
        private readonly CreateStreetcodeCategoryHandler _handler;
        public CreateStreetcodeCategoryContentHandlerTests()
        {
            _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILoggerService>();
            _handler = new CreateStreetcodeCategoryHandler(
                _mockLogger.Object,
                _mockMapper.Object,
                _mockRepositoryWrapper.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnOk_WhenCreateIsSuccess()
        {
            var createDto = new CategoryContentCreateDTO
            {
                StreetcodeId = 1,
                SourceLinkCategoryId = 2
            };
            var entity = new DAL.Entities.Sources.StreetcodeCategoryContent
            {
                StreetcodeId = 1,
                SourceLinkCategoryId = 2
            };

            var returnsDto = new StreetcodeCategoryContentDTO
            {
                StreetcodeId = 1,
                SourceLinkCategoryId = 2
            };

            _mockMapper.Setup(m => m.Map<DAL.Entities.Sources.StreetcodeCategoryContent>(createDto))
                .Returns(entity);

            _mockRepositoryWrapper.Setup(r => r.StreetcodeCategoryContentRepository
            .GetFirstOrDefaultAsync(
               It.IsAny<Expression<Func<DAL.Entities.Sources.StreetcodeCategoryContent, bool>>>(), null))
                .ReturnsAsync((DAL.Entities.Sources.StreetcodeCategoryContent)null);

            _mockRepositoryWrapper.Setup(r => r.StreetcodeCategoryContentRepository.CreateAsync(entity))
                .ReturnsAsync(entity);

            _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

            _mockMapper.Setup(m => m.Map<StreetcodeCategoryContentDTO>(entity))
                .Returns(returnsDto);

            var result = await _handler.Handle(new CreateStreetcodeCategoryContentCommand(createDto), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(returnsDto);
            result.Value.Should().Be(returnsDto);
            _mockMapper.Verify(m => m.Map<DAL.Entities.Sources.StreetcodeCategoryContent>(createDto), Times.Once);
            _mockRepositoryWrapper.Verify(r => r.StreetcodeCategoryContentRepository.CreateAsync(entity), Times.Once);
            _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Once);
            _mockMapper.Verify(m => m.Map<StreetcodeCategoryContentDTO>(entity), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_When_StreetcodeCategoryContentAlreadyExist()
        {
            var createDto = new CategoryContentCreateDTO
            {
                StreetcodeId = 1,
                SourceLinkCategoryId = 2
            };
            var entity = new DAL.Entities.Sources.StreetcodeCategoryContent
            {
                StreetcodeId = 1,
                SourceLinkCategoryId = 2
            };

            _mockMapper.Setup(m => m.Map<DAL.Entities.Sources.StreetcodeCategoryContent>(createDto))
                .Returns(entity);

            _mockRepositoryWrapper.Setup(r => r.StreetcodeCategoryContentRepository
            .GetFirstOrDefaultAsync(
               It.IsAny<Expression<Func<DAL.Entities.Sources.StreetcodeCategoryContent, bool>>>(), null))
                .ReturnsAsync(entity);

            var result = await _handler.Handle(new CreateStreetcodeCategoryContentCommand(createDto), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle(e =>
            e.Message == "Category with this name already exist.");
            _mockMapper.Verify(m => m.Map<DAL.Entities.Sources.StreetcodeCategoryContent>(createDto), Times.Once);
            _mockRepositoryWrapper.Verify(r => r.StreetcodeCategoryContentRepository.CreateAsync(entity), Times.Never);
            _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Never);
            _mockMapper.Verify(m => m.Map<StreetcodeCategoryContentDTO>(entity), Times.Never);
        }

        [Fact]
        public async Task Handle_SHouldReturnFail_WhileCreating()
        {
            var createDto = new CategoryContentCreateDTO
            {
                StreetcodeId = 1,
                SourceLinkCategoryId = 2
            };
            var entity = new DAL.Entities.Sources.StreetcodeCategoryContent
            {
                StreetcodeId = 1,
                SourceLinkCategoryId = 2
            };

            _mockMapper.Setup(m => m.Map<DAL.Entities.Sources.StreetcodeCategoryContent>(createDto))
                .Returns(entity);

            _mockRepositoryWrapper.Setup(r => r.StreetcodeCategoryContentRepository
            .GetFirstOrDefaultAsync(
               It.IsAny<Expression<Func<DAL.Entities.Sources.StreetcodeCategoryContent, bool>>>(), null))
                .ReturnsAsync((DAL.Entities.Sources.StreetcodeCategoryContent)null);

            _mockRepositoryWrapper.Setup(r => r.StreetcodeCategoryContentRepository.CreateAsync(entity))
                .ReturnsAsync(entity);

            _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0);

            var result = await _handler.Handle(new CreateStreetcodeCategoryContentCommand(createDto), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle(e =>
            e.Message == "Failed to create a category content");
            _mockMapper.Verify(m => m.Map<DAL.Entities.Sources.StreetcodeCategoryContent>(createDto), Times.Once);
            _mockRepositoryWrapper.Verify(r => r.StreetcodeCategoryContentRepository.CreateAsync(entity), Times.Once);
            _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Once);
            _mockMapper.Verify(m => m.Map<StreetcodeCategoryContentDTO>(entity), Times.Never);
        }
    }
}
