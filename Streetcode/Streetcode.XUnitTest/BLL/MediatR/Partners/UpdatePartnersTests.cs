using AutoMapper;
using FluentAssertions;
using Moq;
using Streetcode.BLL.DTO.Partners;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Partners.Update;
using Streetcode.DAL.Entities.Partners;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Repositories.Interfaces.Partners;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.Partners;

public class UpdatePartnersTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
    private readonly Mock<IPartnersRepository> _mockPartnersRepository;
    private readonly Mock<IPartnerSourceLinkRepository> _mockPartnerSourceLinkRepository;
    private readonly Mock<IPartnerStreetcodeRepository> _mockPartnerStreetcodeRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly UpdatePartnerHandler _handler;

    public UpdatePartnersTests()
    {
        _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
        _mockPartnersRepository = new Mock<IPartnersRepository>();
        _mockPartnerSourceLinkRepository = new Mock<IPartnerSourceLinkRepository>();
        _mockPartnerStreetcodeRepository = new Mock<IPartnerStreetcodeRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILoggerService>();

        _mockRepositoryWrapper.Setup(r => r.PartnersRepository).Returns(_mockPartnersRepository.Object);
        _mockRepositoryWrapper.Setup(r => r.PartnerSourceLinkRepository).Returns(_mockPartnerSourceLinkRepository.Object);
        _mockRepositoryWrapper.Setup(r => r.PartnerStreetcodeRepository).Returns(_mockPartnerStreetcodeRepository.Object);

        _handler = new UpdatePartnerHandler(
            _mockRepositoryWrapper.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task UpdatePartner_WhenValidDTO_ShouldReturnSuccessWithDTO()
    {
        // Arrange
        var createDto = CreatePartnerDTO();
        var partnerEntity = CreatePartnerEntity();
        var partnerDto = new PartnerDTO { Id = 1, Title = "Updated Partner", Streetcodes = createDto.Streetcodes };

        _mockMapper.Setup(m => m.Map<Partner>(createDto)).Returns(partnerEntity);
        _mockPartnerSourceLinkRepository.Setup(r => r.GetAllAsync(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<PartnerSourceLink, bool>>>(),
            It.IsAny<System.Func<IQueryable<PartnerSourceLink>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<PartnerSourceLink, object>>?>()))
            .ReturnsAsync(new List<PartnerSourceLink>());
        _mockPartnersRepository.Setup(r => r.Update(partnerEntity));
        _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
        _mockPartnerStreetcodeRepository.Setup(r => r.GetAllAsync(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<StreetcodePartner, bool>>>(),
            null))
            .ReturnsAsync(new List<StreetcodePartner>());
        _mockMapper.Setup(m => m.Map<PartnerDTO>(partnerEntity)).Returns(partnerDto);

        var command = new UpdatePartnerQuery(createDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(partnerDto);

        _mockPartnersRepository.Verify(r => r.Update(partnerEntity), Times.Once);
        _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Exactly(2));
    }

    [Fact]
    public async Task UpdatePartner_WhenMapperReturnsNull_ShouldReturnFailure()
    {
        // Arrange
        var createDto = CreatePartnerDTO();

        _mockMapper.Setup(m => m.Map<Partner>(createDto)).Returns((Partner)null);

        var command = new UpdatePartnerQuery(createDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();

        _mockPartnersRepository.Verify(r => r.Update(It.IsAny<Partner>()), Times.Never);
        _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdatePartner_WhenSaveChangesFails_ShouldReturnFailure()
    {
        // Arrange
        var createDto = CreatePartnerDTO();
        var partnerEntity = CreatePartnerEntity();

        _mockMapper.Setup(m => m.Map<Partner>(createDto)).Returns(partnerEntity);
        _mockPartnerSourceLinkRepository.Setup(r => r.GetAllAsync(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<PartnerSourceLink, bool>>>(),
            It.IsAny<System.Func<IQueryable<PartnerSourceLink>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<PartnerSourceLink, object>>?>()))
            .ReturnsAsync(new List<PartnerSourceLink>());
        _mockPartnersRepository.Setup(r => r.Update(partnerEntity));
        _mockRepositoryWrapper.SetupSequence(r => r.SaveChangesAsync())
            .ReturnsAsync(0) // First call fails
            .ReturnsAsync(0); // Second call fails
        _mockPartnerStreetcodeRepository.Setup(r => r.GetAllAsync(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<StreetcodePartner, bool>>>(),
            null))
            .ReturnsAsync(new List<StreetcodePartner>());

        var command = new UpdatePartnerQuery(createDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();

        _mockPartnersRepository.Verify(r => r.Update(partnerEntity), Times.Once);
        _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.AtLeastOnce());
        _mockLogger.Verify(l => l.LogError(command, It.IsAny<string>()), Times.AtLeastOnce());
    }

    [Fact]
    public async Task UpdatePartner_WhenRepositoryThrowsException_ShouldReturnFailure()
    {
        // Arrange
        var createDto = CreatePartnerDTO();
        var partnerEntity = CreatePartnerEntity();

        _mockMapper.Setup(m => m.Map<Partner>(createDto)).Returns(partnerEntity);
        _mockPartnerSourceLinkRepository.Setup(r => r.GetAllAsync(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<PartnerSourceLink, bool>>>(),
            It.IsAny<System.Func<IQueryable<PartnerSourceLink>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<PartnerSourceLink, object>>?>()))
            .ThrowsAsync(new Exception("Repository error"));

        var command = new UpdatePartnerQuery(createDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Message.Contains("Repository error"));
        _mockLogger.Verify(l => l.LogError(command, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task UpdatePartner_ShouldDeleteOldLinksAndStreetcodes()
    {
        // Arrange
        var createDto = CreatePartnerDTO();
        var partnerEntity = CreatePartnerEntity();
        var partnerDto = new PartnerDTO { Id = 1, Title = "Updated Partner", Streetcodes = createDto.Streetcodes };

        // Existing links and streetcodes in DB
        var existingLinks = new List<PartnerSourceLink>
        {
            new PartnerSourceLink { Id = 1, PartnerId = 1 }
        };
        var existingStreetcodes = new List<StreetcodePartner>
        {
            new StreetcodePartner { PartnerId = 1, StreetcodeId = 2 } // Not in newStreetcodeIds
        };

        // Only new link with Id=2 should remain
        partnerEntity.PartnerSourceLinks = new List<PartnerSourceLink>
        {
            new PartnerSourceLink { Id = 2, PartnerId = 1 }
        };

        _mockMapper.Setup(m => m.Map<Partner>(createDto)).Returns(partnerEntity);
        _mockPartnerSourceLinkRepository.Setup(r => r.GetAllAsync(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<PartnerSourceLink, bool>>>(),
            It.IsAny<System.Func<IQueryable<PartnerSourceLink>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<PartnerSourceLink, object>>?>()))
            .ReturnsAsync(existingLinks);
        _mockPartnersRepository.Setup(r => r.Update(partnerEntity));
        _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
        _mockPartnerStreetcodeRepository.Setup(r => r.GetAllAsync(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<StreetcodePartner, bool>>>(),
            null))
            .ReturnsAsync(existingStreetcodes);
        _mockMapper.Setup(m => m.Map<PartnerDTO>(partnerEntity)).Returns(partnerDto);

        var command = new UpdatePartnerQuery(createDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockPartnerSourceLinkRepository.Verify(r => r.Delete(existingLinks[0]), Times.Once);
        _mockPartnerStreetcodeRepository.Verify(r => r.Delete(existingStreetcodes[0]), Times.Once);
    }

    private static CreatePartnerDTO CreatePartnerDTO()
        => new CreatePartnerDTO
        {
            Title = "Updated Partner",
            Streetcodes = new List<Streetcode.BLL.DTO.Streetcode.StreetcodeShortDTO>
            {
                new Streetcode.BLL.DTO.Streetcode.StreetcodeShortDTO { Id = 1, Title = "Streetcode" }
            }
        };

    private static Partner CreatePartnerEntity()
        => new Partner
        {
            Id = 1,
            Title = "Updated Partner",
            Streetcodes = new List<Streetcode.DAL.Entities.Streetcode.StreetcodeContent>()
        };
}