using System.Linq.Expressions;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using Moq;
using Streetcode.BLL.DTO.Toponyms;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Toponyms.GetAll;
using Streetcode.DAL.Entities.Toponyms;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.Toponyms.GetAll
{
    public class GetAllToponymsHandlerTests
    {
        private readonly IFixture _fixture;
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILoggerService> _loggerServiceMock;
        private readonly GetAllToponymsHandler _sut;

        public GetAllToponymsHandlerTests()
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

            _sut = new GetAllToponymsHandler(
                _repositoryWrapperMock.Object,
                _mapperMock.Object,
                _loggerServiceMock.Object);
        }

        [Fact]
        public async Task Handle_WhenToponymsExistWithoutFilter_ReturnsOkResultWithMappedDtos()
        {
            // Arrange
            var entities = _fixture.CreateMany<Toponym>(2).AsQueryable();
            var dtos = entities.Select(e => new ToponymDTO { StreetName = e.StreetName }).ToList();

            _repositoryWrapperMock
                .Setup(r => r.ToponymRepository.FindAll(It.IsAny<Expression<Func<Toponym, bool>>>()))
                .Returns(entities);

            _mapperMock
                .Setup(m => m.Map<IEnumerable<ToponymDTO>>(entities))
                .Returns(dtos);

            var query = new GetAllToponymsQuery(new GetAllToponymsRequestDTO { Title = null });

            // Act
            var result = await _sut.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(dtos.Count, result.Value.Toponyms.Count());

            _repositoryWrapperMock
                .Verify(
                    r => r.ToponymRepository
                .FindAll(It.IsAny<Expression<Func<Toponym, bool>>>()), Times.Once);

            _mapperMock
                .Verify(m => m.Map<IEnumerable<ToponymDTO>>(entities), Times.Once);

            _loggerServiceMock
                .VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_WhenFilterByTitleApplied_ReturnsFilteredDtos()
        {
            // Arrange
            var entities = _fixture.CreateMany<Toponym>(5).ToList();
            entities[0].StreetName = "Shevchenko Street";

            _repositoryWrapperMock
                .Setup(r => r.ToponymRepository.FindAll(It.IsAny<Expression<Func<Toponym, bool>>>()))
                .Returns(entities.AsQueryable());

            _mapperMock
                .Setup(m => m.Map<IEnumerable<ToponymDTO>>(It.IsAny<IEnumerable<Toponym>>()))
                .Returns((IEnumerable<Toponym> src) =>
                    src.Select(t => new ToponymDTO { StreetName = t.StreetName }));

            var query = new GetAllToponymsQuery(new GetAllToponymsRequestDTO { Title = "Shevchenko" });

            // Act
            var result = await _sut.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Contains(result.Value.Toponyms, t => t.StreetName.Contains("Shevchenko"));

            _repositoryWrapperMock
                .Verify(
                    r => r.ToponymRepository
                .FindAll(It.IsAny<Expression<Func<Toponym, bool>>>()), Times.Once);

            _mapperMock
                .Verify(m => m.Map<IEnumerable<ToponymDTO>>(It.IsAny<IEnumerable<Toponym>>()), Times.Once);

            _loggerServiceMock.VerifyNoOtherCalls();
        }
    }
}
