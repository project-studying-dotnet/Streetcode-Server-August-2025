using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Toponyms;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Toponyms.GetByStreetcodeId;
using Streetcode.DAL.Entities.Toponyms;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.Toponyms.GetByStreetCodeId
{
    public class GetToponymsByStreetcodeIdHandlerTests
    {
        private readonly IFixture _fixture;
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILoggerService> _loggerServiceMock;
        private readonly GetToponymsByStreetcodeIdHandler _sut;

        public GetToponymsByStreetcodeIdHandlerTests()
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

            _sut = new GetToponymsByStreetcodeIdHandler(
                _repositoryWrapperMock.Object,
                _mapperMock.Object,
                _loggerServiceMock.Object);
        }

        [Fact]
        public async Task Handle_WhenToponymsExist_ReturnsMappedDtos()
        {
            // Arrange
            var streetcodeId = _fixture.Create<int>();
            var entities = _fixture.Build<Toponym>()
                .Without(t => t.Coordinate)
                .CreateMany(3).ToList();

            _repositoryWrapperMock
                .Setup(r => r.ToponymRepository.GetAllAsync(
                    It.IsAny<Expression<Func<Toponym, bool>>>(),
                    It.IsAny<Func<IQueryable<Toponym>, IIncludableQueryable<Toponym, object>>>()))
                .ReturnsAsync(entities);

            _mapperMock
                .Setup(m => m.Map<ToponymDTO>(It.IsAny<Toponym>()))
                .Returns<Toponym>(t => _fixture.Build<ToponymDTO>()
                                               .With(d => d.StreetName, t.StreetName)
                                               .Create());

            var query = _fixture.Build<GetToponymsByStreetcodeIdQuery>()
                .With(q => q.StreetcodeId, streetcodeId)
                .Create();

            // Act
            var result = await _sut.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(entities.Count, result.Value.Count());

            _repositoryWrapperMock
                .Verify(
                    r => r.ToponymRepository.GetAllAsync(
                    It.IsAny<Expression<Func<Toponym, bool>>>(),
                    It.IsAny<Func<IQueryable<Toponym>, IIncludableQueryable<Toponym, object>>>()), Times.Once);

            _mapperMock.Verify(m => m.Map<ToponymDTO>(It.IsAny<Toponym>()), Times.Exactly(entities.Count));
            _loggerServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_WhenToponymsHaveDuplicatesByStreetName_RemovesDuplicates()
        {
            // Arrange
            var streetcodeId = _fixture.Create<int>();
            var duplicateName = "Shevchenko Street";

            var entities = _fixture.Build<Toponym>()
                .Without(t => t.Coordinate)
                .CreateMany(3).ToList();

            entities[0].StreetName = duplicateName;
            entities[1].StreetName = duplicateName;

            _repositoryWrapperMock
                .Setup(r => r.ToponymRepository.GetAllAsync(
                    It.IsAny<Expression<Func<Toponym, bool>>>(),
                    It.IsAny<Func<IQueryable<Toponym>, IIncludableQueryable<Toponym, object>>>()))
                .ReturnsAsync(entities);

            _mapperMock
                .Setup(m => m.Map<ToponymDTO>(It.IsAny<Toponym>()))
                .Returns<Toponym>(t => _fixture.Build<ToponymDTO>()
                                               .With(d => d.StreetName, t.StreetName)
                                               .Create());

            var query = _fixture.Build<GetToponymsByStreetcodeIdQuery>()
                .With(q => q.StreetcodeId, streetcodeId)
                .Create();

            // Act
            var result = await _sut.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.Count());

            Assert.Contains(result.Value, t => t.StreetName == duplicateName);

            _loggerServiceMock.VerifyNoOtherCalls();
        }
    }
}
