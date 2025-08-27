using AutoMapper;
using FluentResults;
using Moq;
using Streetcode.BLL.DTO.Team;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Team.GetAll;
using Streetcode.DAL.Entities.Team;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Repositories.Interfaces.Team;
using Streetcode.DAL.Specifications.Team;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.Team
{
    public class GetAllTeamHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<ITeamRepository> _teamRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly GetAllTeamHandler _handler;

        public GetAllTeamHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _teamRepositoryMock = new Mock<ITeamRepository>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILoggerService>();

            _repositoryWrapperMock
                .SetupGet(r => r.TeamRepository)
                .Returns(_teamRepositoryMock.Object);

            _handler = new GetAllTeamHandler(
                _repositoryWrapperMock.Object,
                _mapperMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnMappedTeamMembers_WhenTeamExists()
        {
            // Arrange
            var request = new GetAllTeamQuery();

            var teamEntities = new List<TeamMember>
        {
            new TeamMember { Id = 1, FirstName = "John", Description = "Backend Dev", IsMain = true, ImageId = 10 },
            new TeamMember { Id = 2, FirstName = "Jane", Description = "Frontend Dev", IsMain = false, ImageId = 11 }
        };

            var teamDtos = new List<TeamMemberDTO>
        {
            new TeamMemberDTO { Id = 1, FirstName = "John", Description = "Backend Dev", IsMain = true, ImageId = 10 },
            new TeamMemberDTO { Id = 2, FirstName = "Jane", Description = "Frontend Dev", IsMain = false, ImageId = 11 }
        };

            _teamRepositoryMock
                .Setup(r => r.ListAsync(It.IsAny<AllTeamSpecification>(), default))
                .ReturnsAsync(teamEntities);

            _mapperMock
                .Setup(m => m.Map<IEnumerable<TeamMemberDTO>>(teamEntities))
                .Returns(teamDtos);

            // Act
            var result = await _handler.Handle(request, default);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.Count());
            Assert.Contains(result.Value, t => t.FirstName == "John");
            Assert.Contains(result.Value, t => t.FirstName == "Jane");

            _teamRepositoryMock.Verify(r => r.ListAsync(It.IsAny<AllTeamSpecification>(), default), Times.Once);
            _mapperMock.Verify(m => m.Map<IEnumerable<TeamMemberDTO>>(teamEntities), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_WhenNoTeamFound()
        {
            // Arrange
            var request = new GetAllTeamQuery();

            _teamRepositoryMock
                .Setup(r => r.ListAsync(It.IsAny<AllTeamSpecification>(), default))
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
