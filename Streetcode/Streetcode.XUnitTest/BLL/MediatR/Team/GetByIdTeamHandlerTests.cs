using AutoMapper;
using FluentResults;
using Moq;
using Streetcode.BLL.DTO.Team;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Team.GetById;
using Streetcode.DAL.Entities.Team;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Specifications.Team;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.Team
{
    public class GetByIdTeamHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly GetByIdTeamHandler _handler;

        public GetByIdTeamHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILoggerService>();

            _handler = new GetByIdTeamHandler(
                _repositoryWrapperMock.Object,
                _mapperMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnTeamMember_WhenExists()
        {
            // Arrange
            var request = new GetByIdTeamQuery(1);

            var teamEntity = new TeamMember
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Description = "Backend Developer",
                IsMain = true,
                ImageId = 10
            };

            var teamDto = new TeamMemberDTO
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Description = "Backend Developer",
                IsMain = true,
                ImageId = 10
            };

            _repositoryWrapperMock
                .Setup(r => r.TeamRepository.GetBySpecAsync(It.IsAny<TeamByIdSpecification>(), default))
                .ReturnsAsync(teamEntity);

            _mapperMock
                .Setup(m => m.Map<TeamMemberDTO>(teamEntity))
                .Returns(teamDto);

            // Act
            var result = await _handler.Handle(request, default);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(1, result.Value.Id);
            Assert.Equal("John", result.Value.FirstName);
            Assert.Equal("Doe", result.Value.LastName);

            _repositoryWrapperMock.Verify(r => r.TeamRepository.GetBySpecAsync(It.IsAny<TeamByIdSpecification>(), default), Times.Once);
            _mapperMock.Verify(m => m.Map<TeamMemberDTO>(teamEntity), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_WhenTeamNotFound()
        {
            // Arrange
            var request = new GetByIdTeamQuery(99);

            _repositoryWrapperMock
                .Setup(r => r.TeamRepository.GetBySpecAsync(It.IsAny<TeamByIdSpecification>(), default))
                .ReturnsAsync((TeamMember?)null);

            // Act
            var result = await _handler.Handle(request, default);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(result.Errors, e => e.Message.Contains("Cannot find any team with corresponding id: 99"));

            _loggerMock.Verify(l => l.LogError(request, It.IsAny<string>()), Times.Once);
        }
    }
}