using AutoMapper;
using FluentResults;
using Moq;
using Streetcode.BLL.DTO.Team;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Team.GetAll;
using Streetcode.DAL.Entities.Team;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Specifications.Team;
using Xunit;
using TeamEntity = Streetcode.DAL.Entities.Team.TeamMember;

namespace Streetcode.XUnitTest.BLL.MediatR.Team
{
    public class GetAllTeamHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly GetAllTeamHandler _handler;

        public GetAllTeamHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILoggerService>();

            _handler = new GetAllTeamHandler(
                _repositoryWrapperMock.Object,
                _mapperMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnTeam_WhenTeamExists()
        {
            // Arrange
            var request = new GetAllTeamQuery();

            var teamEntities = new List<TeamMember>
        {
            new TeamMember
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Description = "Backend Developer",
                IsMain = true,
                ImageId = 10
            },
            new TeamMember
            {
                Id = 2,
                FirstName = "Jane",
                LastName = "Smith",
                Description = "UI/UX Designer",
                IsMain = false,
                ImageId = 11
            }
        };

            var teamDtos = new List<TeamMemberDTO>
        {
            new TeamMemberDTO
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Description = "Backend Developer",
                IsMain = true,
                ImageId = 10
            },
            new TeamMemberDTO
            {
                Id = 2,
                FirstName = "Jane",
                LastName = "Smith",
                Description = "UI/UX Designer",
                IsMain = false,
                ImageId = 11
            }
        };

            _repositoryWrapperMock
                .Setup(r => r.TeamRepository.ListAsync(It.IsAny<AllTeamSpecification>(), default))
                .ReturnsAsync(teamEntities);

            _mapperMock
                .Setup(m => m.Map<IEnumerable<TeamMemberDTO>>(teamEntities))
                .Returns(teamDtos);

            // Act
            var result = await _handler.Handle(request, default);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.Count());

            var dto = result.Value.First();
            Assert.Equal("John", dto.FirstName);
            Assert.Equal("Doe", dto.LastName);

            _repositoryWrapperMock.Verify(r => r.TeamRepository.ListAsync(It.IsAny<AllTeamSpecification>(), default), Times.Once);
            _mapperMock.Verify(m => m.Map<IEnumerable<TeamMemberDTO>>(teamEntities), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_WhenTeamIsNull()
        {
            // Arrange
            var request = new GetAllTeamQuery();

            _repositoryWrapperMock
                .Setup(r => r.TeamRepository.ListAsync(It.IsAny<AllTeamSpecification>(), default))
                .ReturnsAsync((IEnumerable<TeamMember>?)null);

            // Act
            var result = await _handler.Handle(request, default);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(result.Errors, e => e.Message.Contains("Cannot find any team"));

            _loggerMock.Verify(l => l.LogError(request, It.IsAny<string>()), Times.Once);
        }
    }
}
