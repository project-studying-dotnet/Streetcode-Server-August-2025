using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Partners;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Partners.GetByStreetcodeId;
using Streetcode.DAL.Entities.Partners;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Repositories.Interfaces.Partners;
using Streetcode.DAL.Repositories.Interfaces.Streetcode;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.Partners;

public class GetByStreetcodeIdPartnersTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
    private readonly Mock<IPartnersRepository> _mockPartnersRepository;
    private readonly Mock<IStreetcodeRepository> _mockStreetcodeRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly GetPartnersByStreetcodeIdHandler _handler;

    public GetByStreetcodeIdPartnersTests()
    {
        _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
        _mockPartnersRepository = new Mock<IPartnersRepository>();
        _mockStreetcodeRepository = new Mock<IStreetcodeRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILoggerService>();

        _mockRepositoryWrapper.Setup(r => r.PartnersRepository)
            .Returns(_mockPartnersRepository.Object);
        _mockRepositoryWrapper.Setup(r => r.StreetcodeRepository)
            .Returns(_mockStreetcodeRepository.Object);

        _handler = new GetPartnersByStreetcodeIdHandler(
            _mockMapper.Object,
            _mockRepositoryWrapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task GetPartnersByStreetcodeId_WhenStreetcodeAndPartnersExist_ShouldReturnDTOs()
    {
        // Arrange
        int streetcodeId = 1;
        var streetcodeEntity = new StreetcodeContent { Id = streetcodeId };
        var partnersList = new List<Partner>
        {
            new Partner { Id = 1, Title = "Partner 1", Streetcodes = new List<StreetcodeContent> { streetcodeEntity } },
            new Partner { Id = 2, Title = "Partner 2", Streetcodes = new List<StreetcodeContent> { streetcodeEntity } }
        };
        var partnersDTOList = new List<PartnerDTO>
        {
            new PartnerDTO { Id = 1, Title = "Partner 1" },
            new PartnerDTO { Id = 2, Title = "Partner 2" }
        };

        _mockStreetcodeRepository
            .Setup(r => r.GetSingleOrDefaultAsync(It.IsAny<Expression<Func<StreetcodeContent, bool>>>(), null))
            .ReturnsAsync(streetcodeEntity);

        _mockPartnersRepository
            .Setup(r => r.GetAllAsync(
                It.IsAny<Expression<Func<Partner, bool>>>(),
                It.IsAny<Func<IQueryable<Partner>, IIncludableQueryable<Partner, object>>>()))
            .ReturnsAsync(partnersList);

        _mockMapper
            .Setup(m => m.Map<IEnumerable<PartnerDTO>>(partnersList))
            .Returns(partnersDTOList);

        var query = new GetPartnersByStreetcodeIdQuery(streetcodeId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(partnersDTOList);
    }

    [Fact]
    public async Task GetPartnersByStreetcodeId_WhenStreetcodeNotFound_ShouldReturnFailure()
    {
        // Arrange
        int streetcodeId = 1;

        _mockStreetcodeRepository
            .Setup(r => r.GetSingleOrDefaultAsync(It.IsAny<Expression<Func<StreetcodeContent, bool>>>(), null))
            .ReturnsAsync((StreetcodeContent?)null);

        var query = new GetPartnersByStreetcodeIdQuery(streetcodeId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();

        _mockLogger.Verify(l => l.LogError(query, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task GetPartnersByStreetcodeId_WhenPartnersNotFound_ShouldReturnFailure()
    {
        // Arrange
        int streetcodeId = 1;
        var streetcodeEntity = new StreetcodeContent { Id = streetcodeId };

        _mockStreetcodeRepository
            .Setup(r => r.GetSingleOrDefaultAsync(It.IsAny<Expression<Func<StreetcodeContent, bool>>>(), null))
            .ReturnsAsync(streetcodeEntity);

        _mockPartnersRepository
            .Setup(r => r.GetAllAsync(
                It.IsAny<Expression<Func<Partner, bool>>>(),
                It.IsAny<Func<IQueryable<Partner>, IIncludableQueryable<Partner, object>>>()))
            .ReturnsAsync((IEnumerable<Partner>?)null!);

        var query = new GetPartnersByStreetcodeIdQuery(streetcodeId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();

        _mockLogger.Verify(l => l.LogError(query, It.IsAny<string>()), Times.Once);
    }
}