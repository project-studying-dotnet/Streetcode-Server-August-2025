using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Moq;
using Streetcode.BLL.DTO.Streetcode;
using Streetcode.BLL.DTO.Partners;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Partners.Create;
using Streetcode.DAL.Entities.Partners;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Repositories.Interfaces.Partners;
using Streetcode.DAL.Repositories.Interfaces.Streetcode;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.Partners;

public class CreatePartnerTests
{
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
    private readonly Mock<IPartnersRepository> _mockPartnersRepository;
    private readonly Mock<IStreetcodeRepository> _mockStreetcodeRepository;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly CreatePartnerHandler _handler;

    public CreatePartnerTests()
    {
        _mockMapper = new Mock<IMapper>();
        _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
        _mockPartnersRepository = new Mock<IPartnersRepository>();
        _mockStreetcodeRepository = new Mock<IStreetcodeRepository>();
        _mockLogger = new Mock<ILoggerService>();

        _mockRepositoryWrapper.Setup(r => r.PartnersRepository)
            .Returns(_mockPartnersRepository.Object);
        _mockRepositoryWrapper.Setup(r => r.StreetcodeRepository)
            .Returns(_mockStreetcodeRepository.Object);

        _handler = new CreatePartnerHandler(
            _mockRepositoryWrapper.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task CreatePartner_WhenValidData_ShouldReturnSuccessWithPartnerDTO()
    {
        // Arrange
        var partnerDto = CreateValidPartnerDTO();
        var partnerEntity = CreateValidPartnerEntity();
        var createdEntity = CreateValidPartnerEntity(id: 1);
        var expectedDto = CreateValidPartnerDTO_ForResult(id: 1);

        var query = new CreatePartnerQuery(partnerDto);

        _mockMapper.Setup(m => m.Map<Partner>(partnerDto))
            .Returns(partnerEntity);
        _mockPartnersRepository.Setup(r => r.CreateAsync(partnerEntity))
            .ReturnsAsync(createdEntity);
        _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);
        _ = _mockStreetcodeRepository.Setup(r =>
            r.GetAllAsync(It.IsAny<Expression<Func<StreetcodeContent, bool>>>(), null))
            .ReturnsAsync(new List<StreetcodeContent> { new StreetcodeContent { Id = 1 } });
        _mockMapper.Setup(m => m.Map<PartnerDTO>(createdEntity))
            .Returns(expectedDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedDto);

        _mockPartnersRepository.Verify(r => r.CreateAsync(partnerEntity), Times.Once);
        _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Exactly(2)); // called twice
    }

    [Fact]
    public async Task CreatePartner_WhenMapperReturnsNull_ShouldReturnFailure()
    {
        // Arrange
        var partnerDto = CreateValidPartnerDTO();
        var query = new CreatePartnerQuery(partnerDto);

        _mockMapper.Setup(m => m.Map<Partner>(partnerDto))
            .Returns((Partner)null!);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        _mockPartnersRepository.Verify(r => r.CreateAsync(It.IsAny<Partner>()), Times.Never);
    }

    [Fact]
    public async Task CreatePartner_WhenRepositoryThrowsException_ShouldReturnFailureAndLogError()
    {
        // Arrange
        var partnerDto = CreateValidPartnerDTO();
        var partnerEntity = CreateValidPartnerEntity();
        var query = new CreatePartnerQuery(partnerDto);

        _mockMapper.Setup(m => m.Map<Partner>(partnerDto))
            .Returns(partnerEntity);
        _mockPartnersRepository.Setup(r => r.CreateAsync(It.IsAny<Partner>()))
            .ThrowsAsync(new InvalidOperationException("DB error"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Message.Contains("DB error"));

        _mockLogger.Verify(l => l.LogError(query, It.IsAny<string>()), Times.Once);
    }

    // --- helpers ---
    private static CreatePartnerDTO CreateValidPartnerDTO(int id = 0)
    {
        return new CreatePartnerDTO
        {
            Id = id,
            Title = "Test Partner",
            Streetcodes = new List<StreetcodeShortDTO>
            {
                new StreetcodeShortDTO { Id = 1, Title = "Test Streetcode" }
            }
        };
    }

    private static PartnerDTO CreateValidPartnerDTO_ForResult(int id = 0)
    {
        return new PartnerDTO
        {
            Id = id,
            Title = "Test Partner",
            Streetcodes = new List<StreetcodeShortDTO>
            {
                new StreetcodeShortDTO { Id = 1, Title = "Test Streetcode" }
            }
        };
    }

    private static Partner CreateValidPartnerEntity(int id = 0)
    {
        return new Partner
        {
            Id = id,
            Title = "Test Partner",
            Streetcodes = new List<StreetcodeContent>()
        };
    }
}
