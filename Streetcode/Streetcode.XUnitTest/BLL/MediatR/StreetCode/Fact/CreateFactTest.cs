using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Streetcode.TextContent.Fact;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.Fact.Create;
using Streetcode.BLL.Services.Text.Fact;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Entities.Streetcode.TextContent;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Repositories.Interfaces.Streetcode;
using Streetcode.DAL.Repositories.Interfaces.Streetcode.TextContent;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.StreetCode.Fact
{
    public class CreateFactTest
    {
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
        private readonly Mock<IFactRepository> _mockFactRepository;
        private readonly Mock<ILoggerService> _mockLogger;
        private readonly Mock<IStreetcodeRepository> _mockStreetcodeRepository;
        private CreateFactHandler _handler;

        public CreateFactTest()
        {
            _mockMapper = new Mock<IMapper>();
            _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
            _mockFactRepository = new Mock<IFactRepository>();
            _mockLogger = new Mock<ILoggerService>();
            _mockStreetcodeRepository = new Mock<IStreetcodeRepository>();

            _mockRepositoryWrapper.Setup(repo => repo.FactRepository)
                .Returns(_mockFactRepository.Object);

            _mockRepositoryWrapper.Setup(r => r.FactRepository)
                .Returns(_mockFactRepository.Object);

            _mockRepositoryWrapper.Setup(r => r.StreetcodeRepository)
                .Returns(_mockStreetcodeRepository.Object);

            var factAutoOrder = new FactAutoOrder(_mockRepositoryWrapper.Object);

            _handler = new CreateFactHandler(
                _mockMapper.Object,
                _mockRepositoryWrapper.Object,
                _mockLogger.Object,
                factAutoOrder);
        }

        [Fact]
        public async Task CreateFact_WhenValidFactData_ShouldReturnSuccessWithFactDTO()
        {
            // Arrange
            var factDTO = CreateValidFactDTO();
            var factEntity = CreateValidFactEntity();
            var createdEntity = CreateValidFactEntity();
            var expectedResultDTO = CreateValidFactDTO(id: 1);

            var command = new CreateFactCommand(streetcodeId: 1, factDTO);

            _mockMapper.Setup(m => m.Map<DAL.Entities.Streetcode.TextContent.Facts>(factDTO))
                .Returns(factEntity);
            _mockFactRepository.Setup(r => r.Create(factEntity))
                .Returns(createdEntity);
            _mockStreetcodeRepository
                .Setup(r => r.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<StreetcodeContent, bool>>>(),
                    It.IsAny<Func<IQueryable<StreetcodeContent>, IIncludableQueryable<StreetcodeContent, object>>>()))
                .ReturnsAsync(CreateValidStreetcodeEntity());
            _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);
            _mockMapper.Setup(m => m.Map<FactCreateDto>(createdEntity))
                .Returns(expectedResultDTO);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(expectedResultDTO);

            _mockFactRepository.Verify(r => r.Create(factEntity), Times.Once);
            _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.AtLeast(1));
        }

        [Fact]
        public async Task CreateFact_WhenMapperResultNull_ShouldReturnFailureWithErrorMessage()
        {
            // Arrange
            var factDTO = CreateValidFactDTO();
            var command = new CreateFactCommand(streetcodeId: 1, factDTO);

            _mockMapper.Setup(m => m.Map<DAL.Entities.Streetcode.TextContent.Facts>(factDTO))
                .Returns((DAL.Entities.Streetcode.TextContent.Facts)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsFailed.Should().BeTrue();
            result.Errors.Should().NotBeEmpty();

            _mockFactRepository.Verify(r => r.Create(It.IsAny<DAL.Entities.Streetcode.TextContent.Facts>()), Times.Never);
        }

        [Fact]
        public async Task CreateFact_WhenStreetcodeNotFound_ShouldReturnFail()
        {
            // Arrange
            var factDTO = CreateValidFactDTO();
            var factEntity = CreateValidFactEntity();
            var command = new CreateFactCommand(streetcodeId: 1, factDTO);

            _mockStreetcodeRepository.Setup(r => r.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<StreetcodeContent, bool>>>(),
                    It.IsAny<Func<IQueryable<StreetcodeContent>, IIncludableQueryable<StreetcodeContent, object>>>()))
                .ReturnsAsync((StreetcodeContent)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsFailed.Should().BeTrue();

            _mockFactRepository.Verify(r => r.Create(It.IsAny<Facts>()), Times.Never);
            _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task CreateFact_WhenImageIdIsZero_ShouldSetImageIdToNull()
        {
            // Arrange
            var factDTO = CreateValidFactDTO(imageId: 0);
            var factEntity = CreateValidFactEntity(imageId: 0);
            var createdEntity = CreateValidFactEntity(imageId: null);
            var expectedResultDTO = CreateValidFactDTO(id: 1, imageId: null);

            var command = new CreateFactCommand(streetcodeId: 1, factDTO);

            _mockMapper.Setup(m => m.Map<DAL.Entities.Streetcode.TextContent.Facts>(factDTO))
                .Returns(factEntity);

            _mockFactRepository.Setup(r => r.Create(factEntity))
                .Callback<DAL.Entities.Streetcode.TextContent.Facts>(entity => factEntity = entity)
                .Returns(createdEntity);

            _mockStreetcodeRepository
                .Setup(r => r.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<StreetcodeContent, bool>>>(),
                    It.IsAny<Func<IQueryable<StreetcodeContent>, IIncludableQueryable<StreetcodeContent, object>>>()))
                .ReturnsAsync(CreateValidStreetcodeEntity());

            _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            _mockMapper.Setup(m => m.Map<FactCreateDto>(createdEntity))
                .Returns(expectedResultDTO);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(expectedResultDTO);

            _mockFactRepository.Verify(r => r.Create(It.IsAny<Facts>()), Times.AtLeast(1));
            _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.AtLeast(1));
        }

        [Fact]
        public async Task CreateFact_WhenSaveChangesReturnsZero_ShouldReturnFailureWithErrorMessage()
        {
            // Arrange
            var factDTO = CreateValidFactDTO();
            var factEntity = CreateValidFactEntity();
            var command = new CreateFactCommand(streetcodeId: 1, factDTO);

            _mockMapper.Setup(m => m.Map<DAL.Entities.Streetcode.TextContent.Facts>(factDTO))
                .Returns(factEntity);

            _mockFactRepository.Setup(r => r.Create(factEntity)).Returns(factEntity);

            _mockStreetcodeRepository
                .Setup(r => r.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<StreetcodeContent, bool>>>(),
                    It.IsAny<Func<IQueryable<StreetcodeContent>, IIncludableQueryable<StreetcodeContent, object>>>()))
                .ReturnsAsync(CreateValidStreetcodeEntity());

            _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(0);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsFailed.Should().BeTrue();

            _mockFactRepository.Verify(r => r.Create(It.IsAny<DAL.Entities.Streetcode.TextContent.Facts>()), Times.Once);
            _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateFact_WhenExceptionThrown_ShouldReturnFailureWithErrorMessage()
        {
            // Arrange
            var factDTO = CreateValidFactDTO();
            var factEntity = CreateValidFactEntity();
            var command = new CreateFactCommand(streetcodeId: 1, factDTO);

            _mockMapper.Setup(m => m.Map<DAL.Entities.Streetcode.TextContent.Facts>(factDTO))
                .Returns(factEntity);

            _mockStreetcodeRepository
                .Setup(r => r.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<StreetcodeContent, bool>>>(),
                    It.IsAny<Func<IQueryable<StreetcodeContent>, IIncludableQueryable<StreetcodeContent, object>>>()))
                .ReturnsAsync(CreateValidStreetcodeEntity());

            _mockFactRepository.Setup(r => r.Create(factEntity))
                .Throws<InvalidOperationException>();

            // act an assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));

            _mockFactRepository.Verify(r => r.Create(It.IsAny<DAL.Entities.Streetcode.TextContent.Facts>()), Times.Once);
            _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        private DAL.Entities.Streetcode.TextContent.Facts CreateValidFactEntity(int id = 1, int? imageId = 1)
        {
            return new DAL.Entities.Streetcode.TextContent.Facts
            {
                Id = 1,
                Title = "Sample Fact",
                FactContent = "This is a sample fact content.",
                ImageId = null,
                StreetcodeId = 1
            };
        }

        private FactCreateDto CreateValidFactDTO(int id = 0, int? imageId = 1)
        {
            return new FactCreateDto
            {
                Title = "Sample Fact",
                FactContent = "This is a sample fact content.",
                ImageId = 1,
                Image = null
            };
        }

        private StreetcodeContent CreateValidStreetcodeEntity()
        {
            return new StreetcodeContent
            {
                Id = 1,
                Title = "Sample Streetcode"
            };
        }
    }
}
