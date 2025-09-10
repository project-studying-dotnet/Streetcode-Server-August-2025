using System.Linq.Expressions;
using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Media.Images;
using Streetcode.BLL.DTO.Streetcode.TextContent.Fact;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Media.Image.Create;
using Streetcode.BLL.MediatR.Streetcode.Fact.Update;
using Streetcode.DAL.Entities.Media.Images;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;
using Streetcode.DAL.Entities.Streetcode.TextContent;

namespace Streetcode.XUnitTest.BLL.MediatR.StreetCode.Fact.Update
{
    public class UpdateFactHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly Mock<IMediator> _mediatorMock;
        private readonly UpdateFactHandler _handler;

        public UpdateFactHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILoggerService>();
            _mediatorMock = new Mock<IMediator>();
            _handler = new UpdateFactHandler(
                _repositoryWrapperMock.Object,
                _mapperMock.Object,
                _loggerMock.Object,
                _mediatorMock.Object);
        }

        [Fact]
        public async Task Handle_FactNotFound_ReturnsFailAndLogsError()
        {
            // Arrange

            FactUpdateCreateDto fact = new FactUpdateCreateDto { Id = 1 };

            string errorMsg = $"Fact with Id {fact.Id} not found!";

            _repositoryWrapperMock.Setup(r => r.FactRepository.GetSingleOrDefaultAsync(
                It.IsAny<Expression<Func<Facts, bool>>>(),
                It.IsAny<Func<IQueryable<Facts>, IIncludableQueryable<Facts, object>>>()))
                .ReturnsAsync((Facts)null!);

            var command = new UpdateFactCommand(new FactUpdateCreateDto { Id = 1 });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(result.Errors, e => e.Message.Contains(errorMsg));

            _repositoryWrapperMock.Verify(
                r => r.FactRepository.GetSingleOrDefaultAsync(
                    It.IsAny<Expression<Func<Facts, bool>>>(),
                    It.IsAny<Func<IQueryable<Facts>, IIncludableQueryable<Facts, object>>>()), Times.Once);

            _loggerMock.Verify(l => l.LogError(command, It.Is<string>(s => s.Contains(errorMsg))), Times.Once);
            _mapperMock.VerifyNoOtherCalls();
            _mediatorMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_CreateImageFails_ReturnsFail()
        {
            // Arrange
            var fact = new Facts { Id = 2, Title = "Old Title" };
            var factDto = new FactUpdateCreateDto { Id = fact.Id, Title = fact.Title, NewImage = new ImageFileBaseCreateDTO() };
            const string errorMsg = "Failed to create an image";

            _repositoryWrapperMock.Setup(r => r.FactRepository.GetSingleOrDefaultAsync(
               It.IsAny<Expression<Func<Facts, bool>>>(),
               It.IsAny<Func<IQueryable<Facts>, IIncludableQueryable<Facts, object>>>()))
               .ReturnsAsync(new Facts { Id = 2 });

            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateImageCommand>(), CancellationToken.None))
                .ReturnsAsync(Result.Fail<ImageDTO>(new Error(errorMsg)));

            var command = new UpdateFactCommand(factDto);

            // Act
            var result = await _handler.Handle(command, default);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(result.Errors, e => e.Message.Contains(errorMsg));

            _repositoryWrapperMock.Verify(
                r => r.FactRepository.GetSingleOrDefaultAsync(
                    It.IsAny<Expression<Func<Facts, bool>>>(),
                    It.IsAny<Func<IQueryable<Facts>, IIncludableQueryable<Facts, object>>>()), Times.Once);
            _mediatorMock.Verify(m => m.Send(It.IsAny<CreateImageCommand>(), CancellationToken.None), Times.Once);
            _repositoryWrapperMock.Verify(r => r.FactRepository.Update(It.IsAny<Facts>()), Times.Never);
            _repositoryWrapperMock.Verify(r => r.SaveChangesAsync(), Times.Never);
            _loggerMock.VerifyNoOtherCalls();
            _mapperMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_CreateImageSucceed_ReturnsSuccess()
        {
            // Arrange
            const string imageDescription = "Alt desc";
            const int imageId = 42;
            var fact = new Facts { Id = 4, ImageId = imageId };
            var factDto = new FactUpdateCreateDto
            {
                Id = fact.Id,
                NewImage = new ImageFileBaseCreateDTO(),
                ImageDescription = imageDescription
            };
            var expectedResult = new FactDTO { Id = fact.Id, ImageId = imageId };

            SetupRepositories(fact, imageId, imageDescription);
            SetupMediator(imageId);
            SetupMapper(factDto, fact, expectedResult);

            var command = new UpdateFactCommand(factDto);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(expectedResult, result.Value);
            Assert.Equal(imageId, fact.ImageId);
            _mediatorMock.Verify(
                m => m.Send(
                It.Is<CreateImageCommand>(c => c.Image == factDto.NewImage),
                CancellationToken.None), Times.Once);
            VerifyCalls(fact, imageId, imageDescription, factDto);
        }

        [Fact]
        public async Task Handle_UpdateWithoutNewImage_CreatesImageDetailsWhenNotExist()
        {
            // Arrange
            const string imageDescription = "Alt desc";
            const int imageId = 60;

            var fact = new Facts { Id = 6, ImageId = imageId };
            var factDto = new FactUpdateCreateDto { Id = fact.Id, ImageDescription = imageDescription };
            var expectedResult = new FactDTO { Id = fact.Id, ImageId = imageId };

            _repositoryWrapperMock.Setup(r => r.FactRepository.GetSingleOrDefaultAsync(
              It.IsAny<Expression<Func<Facts, bool>>>(),
              It.IsAny<Func<IQueryable<Facts>, IIncludableQueryable<Facts, object>>>()))
              .ReturnsAsync(fact);

            _repositoryWrapperMock.Setup(r => r.ImageDetailsRepository.GetSingleOrDefaultAsync(
                It.IsAny<Expression<Func<ImageDetails, bool>>>(),
                It.IsAny<Func<IQueryable<ImageDetails>, IIncludableQueryable<ImageDetails, object>>>()))
                .ReturnsAsync((ImageDetails?)null);

            _repositoryWrapperMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

            _mapperMock.Setup(m => m.Map(factDto, fact)).Returns(fact);
            _mapperMock.Setup(m => m.Map<FactDTO>(fact)).Returns(expectedResult);

            var command = new UpdateFactCommand(factDto);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(expectedResult, result.Value);

            _repositoryWrapperMock.Verify(
                r => r.ImageDetailsRepository.CreateAsync(
                    It.Is<ImageDetails>(i => i.ImageId == imageId && i.Alt == imageDescription)), Times.Once);

            _repositoryWrapperMock.Verify(r => r.SaveChangesAsync(), Times.Once);
            _mapperMock.Verify(m => m.Map(factDto, fact), Times.Once);
            _mapperMock.Verify(m => m.Map<FactDTO>(fact), Times.Once);
            _mediatorMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_SaveChangesReturnsZero_ReturnsFailAndLogsError()
        {
            // Arrange
            const string errorMsg = "Failed to update a fact";
            var fact = new Facts { Id = 7 };
            var factDto = new FactUpdateCreateDto { Id = fact.Id };

            _repositoryWrapperMock.Setup(r => r.FactRepository.GetSingleOrDefaultAsync(
              It.IsAny<Expression<Func<Facts, bool>>>(),
              It.IsAny<Func<IQueryable<Facts>, IIncludableQueryable<Facts, object>>>()))
              .ReturnsAsync(fact);

            _repositoryWrapperMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0);

            _mapperMock.Setup(m => m.Map(factDto, fact)).Returns(fact);
            _mapperMock.Setup(m => m.Map<FactDTO>(fact)).Returns(new FactDTO { Id = fact.Id });

            var command = new UpdateFactCommand(factDto);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(result.Errors, e => e.Message.Contains(errorMsg));

            _repositoryWrapperMock.Verify(r => r.FactRepository.Update(fact), Times.Once);
            _repositoryWrapperMock.Verify(r => r.SaveChangesAsync(), Times.Once);
            _loggerMock.Verify(l => l.LogError(fact, It.Is<string>(s => s.Contains(errorMsg))), Times.Once);

            _mapperMock.Verify(m => m.Map(factDto, fact), Times.Once);
            _mapperMock.Verify(m => m.Map<FactDTO>(fact), Times.Never);
            _mediatorMock.VerifyNoOtherCalls();
        }

        private void SetupRepositories(Facts fact, int imageId, string imageDesc)
        {
            _repositoryWrapperMock.Setup(r => r.FactRepository.GetSingleOrDefaultAsync(
                It.IsAny<Expression<Func<Facts, bool>>>(), It.IsAny<Func<IQueryable<Facts>, IIncludableQueryable<Facts, object>>>()))
                .ReturnsAsync(fact);

            _repositoryWrapperMock.Setup(r => r.ImageDetailsRepository.GetSingleOrDefaultAsync(
                It.IsAny<Expression<Func<ImageDetails, bool>>>(), It.IsAny<Func<IQueryable<ImageDetails>, IIncludableQueryable<ImageDetails, object>>>()))
                .ReturnsAsync(new ImageDetails { Id = 10, ImageId = imageId, Alt = "Old Alt" });

            _repositoryWrapperMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
        }

        private void SetupMediator(int imageId) =>
            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateImageCommand>(), CancellationToken.None))
                .ReturnsAsync(Result.Ok(new ImageDTO { Id = imageId }));

        private void SetupMapper(FactUpdateCreateDto factDto, Facts fact, FactDTO expected)
        {
            _mapperMock.Setup(m => m.Map(factDto, fact))
                .Returns((FactUpdateCreateDto src, Facts dest) =>
                {
                    dest.Title = src.Title;
                    dest.FactContent = src.FactContent;
                    return dest;
                });

            _mapperMock.Setup(m => m.Map<FactDTO>(fact)).Returns(expected);
        }

        private void VerifyCalls(Facts fact, int imageId, string imageDesc, FactUpdateCreateDto dto)
        {
            _repositoryWrapperMock.Verify(r => r.SaveChangesAsync(), Times.Once);
            _repositoryWrapperMock.Verify(
                r => r.ImageDetailsRepository.Update(
                It.Is<ImageDetails>(id => id.ImageId == imageId && id.Alt == imageDesc)), Times.Once);

            _mapperMock.Verify(m => m.Map(dto, fact), Times.Once);
            _mapperMock.Verify(m => m.Map<FactDTO>(fact), Times.Once);
            _mediatorMock.Verify(m => m.Send(It.IsAny<CreateImageCommand>(), CancellationToken.None), Times.Once);
            _repositoryWrapperMock.Verify(r => r.FactRepository.Update(fact), Times.Once);
            _loggerMock.VerifyNoOtherCalls();
        }
    }
}
