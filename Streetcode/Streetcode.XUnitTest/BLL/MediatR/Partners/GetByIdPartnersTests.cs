using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Partners;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Partners.GetById;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.DAL.Entities.Partners;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Repositories.Interfaces.Partners;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.Partners;

public class GetByIdPartnersTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
    private readonly Mock<IPartnersRepository> _mockPartnersRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly GetPartnerByIdHandler _handler;

    public GetByIdPartnersTests()
    {
        _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
        _mockPartnersRepository = new Mock<IPartnersRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILoggerService>();

        _mockRepositoryWrapper.Setup(r => r.PartnersRepository)
            .Returns(_mockPartnersRepository.Object);

        _handler = new GetPartnerByIdHandler(
            _mockRepositoryWrapper.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task GetPartnerById_WhenPartnerExists_ShouldReturnDTO()
    {
        // Arrange
        int partnerId = 1;
        var partnerEntity = CreatePartnerEntity(partnerId);
        var partnerDTO = CreatePartnerDTO(partnerId);

        _mockPartnersRepository
            .Setup(r => r.GetSingleOrDefaultAsync(
                It.IsAny<Expression<Func<Partner, bool>>>(),
                It.IsAny<Func<IQueryable<Partner>, IIncludableQueryable<Partner, object>>>()))
            .ReturnsAsync(partnerEntity);

        _mockMapper
            .Setup(m => m.Map<PartnerDTO>(partnerEntity))
            .Returns(partnerDTO);

        var query = new GetPartnerByIdQuery(partnerId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(partnerDTO);
        result.Value.Id.Should().Be(partnerId);

        _mockMapper.Verify(m => m.Map<PartnerDTO>(partnerEntity), Times.Once);
    }

    [Fact]
    public async Task GetPartnerById_WhenPartnerNotFound_ShouldReturnFailure()
    {
        // Arrange
        int partnerId = 1;

        _mockPartnersRepository
            .Setup(r => r.GetSingleOrDefaultAsync(
                It.IsAny<Expression<Func<Partner, bool>>>(),
                It.IsAny<Func<IQueryable<Partner>, IIncludableQueryable<Partner, object>>>()))
            .ReturnsAsync((Partner?)null);

        var query = new GetPartnerByIdQuery(partnerId);
        string errorMsg = Errors_Common.NotFoundById.FormatWith("partner", query.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Errors[0].Message.Should().Be(errorMsg);

        _mockLogger.Verify(l => l.LogError(query, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task GetPartnerById_WhenMapperReturnsNull_ShouldReturnFailure()
    {
        // Arrange
        int partnerId = 1;
        var partnerEntity = CreatePartnerEntity(partnerId);
        string errorMsg = Errors_Common.CannotMap.FormatWith("partner");

        _mockPartnersRepository
            .Setup(r => r.GetSingleOrDefaultAsync(
                It.IsAny<Expression<Func<Partner, bool>>>(),
                It.IsAny<Func<IQueryable<Partner>, IIncludableQueryable<Partner, object>>>()))
            .ReturnsAsync(partnerEntity);

        _mockMapper
            .Setup(m => m.Map<PartnerDTO?>(partnerEntity))
            .Returns((PartnerDTO?)null);

        var query = new GetPartnerByIdQuery(partnerId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Errors[0].Message.Should().Be(errorMsg);

        _mockMapper.Verify(m => m.Map<PartnerDTO>(partnerEntity), Times.Once);
    }

    // --- helpers ---
    private static Partner CreatePartnerEntity(int id = 1)
        => new() { Id = id, Title = $"Test Partner {id}" };

    private static PartnerDTO CreatePartnerDTO(int id = 1)
        => new() { Id = id, Title = $"Test Partner {id}" };
}