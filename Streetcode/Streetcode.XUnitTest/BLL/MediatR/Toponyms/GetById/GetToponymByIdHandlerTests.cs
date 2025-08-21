using System.Linq.Expressions;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Toponyms;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Toponyms.GetById;
using Streetcode.DAL.Entities.Toponyms;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.Toponyms.GetById
{
    public class GetToponymByIdHandlerTests
    {
        private readonly IFixture _fixture;
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILoggerService> _loggerServiceMock;
        private readonly GetToponymByIdHandler _sut;

        public GetToponymByIdHandlerTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });

            _fixture.Behaviors
                .OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));

            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _repositoryWrapperMock = _fixture.Freeze<Mock<IRepositoryWrapper>>();
            _mapperMock = _fixture.Freeze<Mock<IMapper>>();
            _loggerServiceMock = _fixture.Freeze<Mock<ILoggerService>>();

            _sut = new GetToponymByIdHandler(
                _repositoryWrapperMock.Object,
                _mapperMock.Object,
                _loggerServiceMock.Object);
        }

        [Fact]
        public async Task Handle_WhenToponymExists_ReturnsOkWithMappedDto()
        {
            // Arrange
            var toponym = _fixture.Create<Toponym>();
            var toponymDto = _fixture.Create<ToponymDTO>();

            var query = _fixture.Build<GetToponymByIdQuery>()
                .With(q => q.Id, toponym.Id)
                .Create();

            _repositoryWrapperMock
                .Setup(r => r.ToponymRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Toponym, bool>>>(),
                    It.IsAny<Func<IQueryable<Toponym>, IIncludableQueryable<Toponym, object>>>()))
                .ReturnsAsync(toponym);

            _mapperMock
                .Setup(m => m.Map<ToponymDTO>(toponym))
                .Returns(toponymDto);

            // Act
            var result = await _sut.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(toponymDto, result.Value);

            _loggerServiceMock.Verify(
                l => l.LogError(It.IsAny<object>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_WhenToponymDoesNotExist_ReturnsFailAndLogsError()
        {
            // Arrange
            var query = _fixture.Create<GetToponymByIdQuery>();

            _repositoryWrapperMock
                .Setup(r => r.ToponymRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Toponym, bool>>>(),
                    It.IsAny<Func<IQueryable<Toponym>, IIncludableQueryable<Toponym, object>>>()))
                .ReturnsAsync((Toponym?)null);

            // Act
            var result = await _sut.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(result.Errors, e => e.Message.Contains(query.Id.ToString()));

            _loggerServiceMock.Verify(
                l => l.LogError(
                    query,
                    It.Is<string>(msg => msg.Contains(query.Id.ToString()))),
                Times.Once);
        }
    }
}
