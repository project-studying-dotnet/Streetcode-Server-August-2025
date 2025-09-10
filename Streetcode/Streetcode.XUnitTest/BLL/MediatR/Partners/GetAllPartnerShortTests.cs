using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Partners;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Partners.GetAllPartnerShort;
using Streetcode.DAL.Entities.Partners;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Repositories.Interfaces.Partners;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.Partners;

public class GetAllPartnerShortTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
    private readonly Mock<IPartnersRepository> _mockPartnersRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly GetAllPartnerShortHandler _handler;

    public GetAllPartnerShortTests()
    {
        _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
        _mockPartnersRepository = new Mock<IPartnersRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILoggerService>();

        _mockRepositoryWrapper.Setup(r => r.PartnersRepository)
            .Returns(_mockPartnersRepository.Object);

        _handler = new GetAllPartnerShortHandler(
            _mockRepositoryWrapper.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public void CanInstantiate_GetAllPartnersShortQuery()
    {
        // Act
        var query = new GetAllPartnersShortQuery();

        // Assert
        query.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllPartnerShort_WhenPartnersExist_ShouldReturnMappedShortDTOs()
    {
        // Arrange
        var partnersList = new List<Partner>
        {
            CreatePartnerEntity(1),
            CreatePartnerEntity(2)
        };
        var partnersShortDTOList = new List<PartnerShortDTO>
        {
            CreatePartnerShortDTO(1),
            CreatePartnerShortDTO(2)
        };

        _mockPartnersRepository
            .Setup(r => r.GetAllAsync(
                It.IsAny<Expression<Func<Partner, bool>>>(),
                It.IsAny<Func<IQueryable<Partner>, IIncludableQueryable<Partner, object>>>()))
            .ReturnsAsync(partnersList);

        _mockMapper
            .Setup(m => m.Map<IEnumerable<PartnerShortDTO>>(partnersList))
            .Returns(partnersShortDTOList);

        var query = new GetAllPartnersShortQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(partnersShortDTOList);

        _mockPartnersRepository.Verify(
            r => r.GetAllAsync(
                It.IsAny<Expression<Func<Partner, bool>>>(),
                It.IsAny<Func<IQueryable<Partner>, IIncludableQueryable<Partner, object>>>()), Times.Once);

        _mockMapper.Verify(m => m.Map<IEnumerable<PartnerShortDTO>>(partnersList), Times.Once);
    }

    [Fact]
    public async Task GetAllPartnerShort_WhenRepositoryReturnsNull_ShouldReturnFailure()
    {
        // Arrange
        _mockPartnersRepository
            .Setup(r => r.GetAllAsync(
                It.IsAny<Expression<Func<Partner, bool>>>(),
                It.IsAny<Func<IQueryable<Partner>, IIncludableQueryable<Partner, object>>>()))
            .ReturnsAsync((IEnumerable<Partner>?)null!);

        var query = new GetAllPartnersShortQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();

        _mockPartnersRepository.Verify(
            r => r.GetAllAsync(
                It.IsAny<Expression<Func<Partner, bool>>>(),
                It.IsAny<Func<IQueryable<Partner>, IIncludableQueryable<Partner, object>>>()), Times.Once);

        _mockLogger.Verify(l => l.LogError(query, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task GetAllPartnerShort_WhenRepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        var expectedException = new Exception("Database connection failed");

        _mockPartnersRepository
            .Setup(r => r.GetAllAsync(
                It.IsAny<Expression<Func<Partner, bool>>>(),
                It.IsAny<Func<IQueryable<Partner>, IIncludableQueryable<Partner, object>>>()))
            .ThrowsAsync(expectedException);

        var query = new GetAllPartnersShortQuery();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            _handler.Handle(query, CancellationToken.None));

        exception.Should().Be(expectedException);
    }

    // --- helpers ---
    private static Partner CreatePartnerEntity(int id = 1)
        => new() { Id = id, Title = $"Test Partner {id}" };

    private static PartnerShortDTO CreatePartnerShortDTO(int id = 1)
        => new() { Id = id, Title = $"Test Partner {id}" };
}