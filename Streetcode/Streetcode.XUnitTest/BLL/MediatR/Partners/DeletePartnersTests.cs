using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Moq;
using Streetcode.BLL.DTO.Partners;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Partners.Delete;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.DAL.Entities.Partners;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Repositories.Interfaces.Partners;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.Partners;

public class DeletePartnerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
    private readonly Mock<IPartnersRepository> _mockPartnersRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly DeletePartnerHandler _handler;

    public DeletePartnerTests()
    {
        _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
        _mockPartnersRepository = new Mock<IPartnersRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILoggerService>();

        _mockRepositoryWrapper.Setup(r => r.PartnersRepository)
            .Returns(_mockPartnersRepository.Object);

        _handler = new DeletePartnerHandler(
            _mockRepositoryWrapper.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task DeletePartner_WhenValidIdAndPartnerExists_ShouldReturnSuccessAndDeletePartner()
    {
        // Arrange
        var command = new DeletePartnerQuery(1);
        var partner = new Partner { Id = 1, Title = "Test Partner" };
        var partnerDto = new PartnerDTO { Id = 1, Title = "Test Partner" };

        _mockPartnersRepository.Setup(r => r.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<Partner, bool>>>(), null))
            .ReturnsAsync(partner);

        _mockMapper.Setup(m => m.Map<PartnerDTO>(partner))
            .Returns(partnerDto);

        _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(partnerDto);

        _mockPartnersRepository.Verify(r => r.Delete(partner), Times.Once);
        _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeletePartner_WhenPartnerNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = new DeletePartnerQuery(1);
        var errorMsg = Errors_Common.NotFoundById.FormatWith("partner", command.id);

        _mockPartnersRepository.Setup(r => r.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<Partner, bool>>>(), null))
            .ReturnsAsync((Partner?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Errors[0].Message.Should().Be(errorMsg);

        _mockPartnersRepository.Verify(r => r.Delete(It.IsAny<Partner>()), Times.Never);
        _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Never);
        _mockLogger.Verify(l => l.LogError(command, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task DeletePartner_WhenRepositoryThrowsException_ShouldReturnFailureAndLogError()
    {
        // Arrange
        var command = new DeletePartnerQuery(1);
        var partner = new Partner { Id = 1, Title = "Test Partner" };

        _mockPartnersRepository.Setup(r => r.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<Partner, bool>>>(), null))
            .ReturnsAsync(partner);

        _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync())
            .ThrowsAsync(new InvalidOperationException("DB error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Message.Contains("DB error"));

        _mockLogger.Verify(l => l.LogError(command, It.IsAny<string>()), Times.Once);
    }
}