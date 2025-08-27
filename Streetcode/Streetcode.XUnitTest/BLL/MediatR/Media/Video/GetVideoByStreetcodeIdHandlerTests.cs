using AutoMapper;
using FluentResults;
using Moq;
using Streetcode.BLL.DTO.Media.Video;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Media.Video.GetByStreetcodeId;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Specifications.Streetcode;
using Streetcode.DAL.Specifications.Video;
using Xunit;
using StreetcodeEntity = Streetcode.DAL.Entities.Streetcode.StreetcodeContent;
using VideoEntity = Streetcode.DAL.Entities.Media.Video;

namespace Streetcode.XUnitTest.BLL.MediatR.Media.Video
{
    public class GetVideoByStreetcodeIdHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILoggerService> _loggerMock;

        private readonly GetVideoByStreetcodeIdHandler _handler;

        public GetVideoByStreetcodeIdHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILoggerService>();

            _handler = new GetVideoByStreetcodeIdHandler(
                _repositoryWrapperMock.Object,
                _mapperMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnVideo_WhenVideoExists()
        {
            // Arrange
            var request = new GetVideoByStreetcodeIdQuery(1);

            var videoEntity = new VideoEntity { Id = 10, Url = "https://test.com/video.mp4" };
            var videoDto = new VideoDTO { Id = 10, Url = "https://test.com/video.mp4" };

            _repositoryWrapperMock
                .Setup(r => r.VideoRepository.GetBySpecAsync(It.IsAny<VideoByStreetCodeIdSpecification>(), default))
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

            _repositoryWrapperMock.Verify(r => r.VideoRepository.GetBySpecAsync(It.IsAny<VideoByStreetCodeIdSpecification>(), default), Times.Once);
            _mapperMock.Verify(m => m.Map<VideoDTO>(videoEntity), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnNullResult_WhenVideoDoesNotExistButStreetcodeExists()
        {
            // Arrange
            var request = new GetVideoByStreetcodeIdQuery(2);

            var streetcodeEntity = new StreetcodeEntity { Id = 2, Title = "Test Streetcode" };

            _repositoryWrapperMock
                .Setup(r => r.VideoRepository.GetBySpecAsync(It.IsAny<VideoByStreetCodeIdSpecification>(), default))
                .ReturnsAsync((VideoEntity?)null);

            _repositoryWrapperMock
                .Setup(r => r.StreetcodeRepository.GetBySpecAsync(It.IsAny<StreetCodeByIdSpecification>(), default))
                .ReturnsAsync(streetcodeEntity);

            _mapperMock
                .Setup(m => m.Map<VideoDTO>((VideoEntity?)null))
                .Returns((VideoDTO?)null);

            // Act
            var result = await _handler.Handle(request, default);

            // Assert
            Assert.True(result.IsSuccess); // NullResult вважається успішним
            Assert.Null(result.Value);

            _repositoryWrapperMock.Verify(r => r.StreetcodeRepository.GetBySpecAsync(It.IsAny<StreetCodeByIdSpecification>(), default), Times.Once);
        }
    }
}