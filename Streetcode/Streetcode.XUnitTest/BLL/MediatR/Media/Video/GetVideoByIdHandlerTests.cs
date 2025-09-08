using AutoMapper;
using Moq;
using Streetcode.BLL.DTO.Media.Video;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Media.Video.GetById;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Specifications.Video;
using Xunit;
using VideoEntity = Streetcode.DAL.Entities.Media.Video;

namespace Streetcode.XUnitTest.BLL.MediatR.Media.Video
{
    public class GetVideoByIdHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILoggerService> _loggerMock;

        private readonly GetVideoByIdHandler _handler;

        public GetVideoByIdHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILoggerService>();

            _handler = new GetVideoByIdHandler(
                _repositoryWrapperMock.Object,
                _mapperMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnVideo_WhenVideoExists()
        {
            // Arrange
            var request = new GetVideoByIdQuery(1);

            var videoEntity = new VideoEntity { Id = 1, Url = "https://test.com/video.mp4" };
            var videoDto = new VideoDTO { Id = 1, Url = "https://test.com/video.mp4" };

            _repositoryWrapperMock
                .Setup(r => r.VideoRepository.GetBySpecAsync(It.IsAny<VideoByIdSpecification>(), default))
                .ReturnsAsync(videoEntity);

            _mapperMock
                .Setup(m => m.Map<VideoDTO>(videoEntity))
                .Returns(videoDto);

            // Act
            var result = await _handler.Handle(request, default);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(videoDto.Id, result.Value.Id);
            Assert.Equal(videoDto.Url, result.Value.Url);

            _repositoryWrapperMock.Verify(r => r.VideoRepository.GetBySpecAsync(It.IsAny<VideoByIdSpecification>(), default), Times.Once);
            _mapperMock.Verify(m => m.Map<VideoDTO>(videoEntity), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_WhenVideoDoesNotExist()
        {
            // Arrange
            var request = new GetVideoByIdQuery(99);

            _repositoryWrapperMock
                .Setup(r => r.VideoRepository.GetBySpecAsync(It.IsAny<VideoByIdSpecification>(), default))
                .ReturnsAsync((VideoEntity?)null);

            // Act
            var result = await _handler.Handle(request, default);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(result.Errors, e => e.Message.Contains("Cannot find any video"));

            _loggerMock.Verify(l => l.LogError(request, It.IsAny<string>()), Times.Once);
        }
    }
}