using System.Linq.Expressions;
using System.Transactions;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Streetcode.Update;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Interfaces.Redis;
using Streetcode.BLL.MediatR.Streetcode.Streetcode.Update;
using Streetcode.DAL.Entities.AdditionalContent;
using Streetcode.DAL.Entities.Media.Images;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.StreetCode.Streetcode.Update
{
    public class UpdateStreetcodeTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILoggerService> _loggerServiceMock;
        private readonly Mock<IRedisService<StreetcodeContent>> _redisServiceMock;
        private readonly UpdateStreetcodeHandler _handler;
        public UpdateStreetcodeTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _loggerServiceMock = new Mock<ILoggerService>();
            _redisServiceMock = new Mock<IRedisService<StreetcodeContent>>();

            _handler = new UpdateStreetcodeHandler(_repositoryWrapperMock.Object, _mapperMock.Object, _loggerServiceMock.Object, _redisServiceMock.Object);
        }

        [Fact]
        public async Task Handle_Should_ReturnOk_WhenUpdateIsSuccessful()
        {
            // Arrange
            var entity = new StreetcodeContent { Id = 10, UpdatedAt = DateTime.MinValue };
            var requestDto = new StreetcodeUpdateDTO { Id = 10 };
            var request = new UpdateStreetcodeCommand(requestDto);

            _repositoryWrapperMock
                .Setup(r => r.StreetcodeRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<StreetcodeContent, bool>>>(),
                    It.IsAny<Func<IQueryable<StreetcodeContent>, IIncludableQueryable<StreetcodeContent, object>>>()))
                .ReturnsAsync(entity);

            _repositoryWrapperMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

            // Мок для маппера
            _mapperMock.Setup(m => m.Map(It.IsAny<StreetcodeUpdateDTO>(), It.IsAny<StreetcodeContent>()))
                .Verifiable();

            using var realTransactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            _repositoryWrapperMock.Setup(r => r.BeginTransaction()).Returns(realTransactionScope);

            SetupAdditionalMocks();

            var result = await _handler.Handle(request, CancellationToken.None);

            Assert.True(result.IsSuccess, $"Result should be successful but failed with: {string.Join(", ", result.Errors.Select(e => e.Message))}");
            Assert.Equal(10, result.Value);
            Assert.True(entity.UpdatedAt > DateTime.MinValue);

            _repositoryWrapperMock.Verify(r => r.SaveChangesAsync(), Times.Once);
            _loggerServiceMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Once);
            _repositoryWrapperMock.Verify(r => r.BeginTransaction(), Times.Once);
            _mapperMock.Verify(m => m.Map(It.IsAny<StreetcodeUpdateDTO>(), It.IsAny<StreetcodeContent>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_ReturnFail_WhenEntityNotFound()
        {
            var request = new UpdateStreetcodeCommand(new StreetcodeUpdateDTO { Id = 1 });

            _repositoryWrapperMock.Setup(r => r.StreetcodeRepository.GetFirstOrDefaultAsync(
               It.IsAny<Expression<Func<StreetcodeContent, bool>>>(),
               It.IsAny<Func<IQueryable<StreetcodeContent>, IIncludableQueryable<StreetcodeContent, object>>>()))
                .ReturnsAsync((StreetcodeContent)null!);

            var result = await _handler.Handle(request, CancellationToken.None);

            Assert.True(result.IsFailed);
            _loggerServiceMock.Verify(l => l.LogError(request, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_WhenSaveChangesFails()
        {
            var entity = new StreetcodeContent { Id = 1 };

            var request = new UpdateStreetcodeCommand(new StreetcodeUpdateDTO { Id = 1 });

            _repositoryWrapperMock.Setup(r => r.StreetcodeRepository.GetFirstOrDefaultAsync(
               It.IsAny<Expression<Func<StreetcodeContent, bool>>>(),
               It.IsAny<Func<IQueryable<StreetcodeContent>, IIncludableQueryable<StreetcodeContent, object>>>()))
                .ReturnsAsync(entity);

            _repositoryWrapperMock.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(0);

            var result = await _handler.Handle(request, CancellationToken.None);

            Assert.True(result.IsFailed);
            _loggerServiceMock.Verify(l => l.LogError(request, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_WhenThrowException()
        {
            var request = new UpdateStreetcodeCommand(new StreetcodeUpdateDTO { Id = 1 });

            _repositoryWrapperMock.Setup(r => r.StreetcodeRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<StreetcodeContent, bool>>>(),
                It.IsAny<Func<IQueryable<StreetcodeContent>, IIncludableQueryable<StreetcodeContent, object>>>()))
                .ThrowsAsync(new Exception("DB error"));

            var result = await _handler.Handle(request, CancellationToken.None);
            Assert.True(result.IsFailed);
            Assert.Contains("DB error", result.Errors[0].Message);
            _loggerServiceMock.Verify(l => l.LogError(request, "DB error"), Times.Once);
        }

        private void SetupAdditionalMocks()
        {
            // Моки для методів оновлення тегів та зображень
            _repositoryWrapperMock.Setup(r => r.StreetcodeTagIndexRepository.CreateRangeAsync(It.IsAny<IEnumerable<StreetcodeTagIndex>>()))
                .Returns(Task.CompletedTask);
            _repositoryWrapperMock.Setup(r => r.StreetcodeTagIndexRepository.DeleteRange(It.IsAny<IEnumerable<StreetcodeTagIndex>>()))
                .Verifiable();
            _repositoryWrapperMock.Setup(r => r.StreetcodeTagIndexRepository.UpdateRange(It.IsAny<IEnumerable<StreetcodeTagIndex>>()))
                .Verifiable();
            _repositoryWrapperMock.Setup(r => r.ImageRepository.DeleteRange(It.IsAny<IEnumerable<Image>>()))
                .Verifiable();
            _repositoryWrapperMock.Setup(r => r.StreetcodeImageRepository.CreateRangeAsync(It.IsAny<IEnumerable<StreetcodeImage>>()))
                .Returns(Task.CompletedTask);
        }
    }
}
