using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Moq;
using Streetcode.BLL.DTO.Transactions;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Transactions.TransactionLink.GetByStreetcodeId;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Entities.Transactions;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Repositories.Interfaces.Streetcode;
using Streetcode.DAL.Repositories.Interfaces.Transactions;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.Transactions;

public class GetTransactLinkByStreetcodeIdHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
    private readonly Mock<ITransactLinksRepository> _mockTransactLinksRepository;
    private readonly Mock<IStreetcodeRepository> _mockStreetcodeRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly GetTransactLinkByStreetcodeIdHandler _handler;

    public GetTransactLinkByStreetcodeIdHandlerTests()
    {
        _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
        _mockTransactLinksRepository = new Mock<ITransactLinksRepository>();
        _mockStreetcodeRepository = new Mock<IStreetcodeRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILoggerService>();

        _mockRepositoryWrapper.Setup(r => r.TransactLinksRepository).Returns(_mockTransactLinksRepository.Object);
        _mockRepositoryWrapper.Setup(r => r.StreetcodeRepository).Returns(_mockStreetcodeRepository.Object);

        _handler = new GetTransactLinkByStreetcodeIdHandler(
            _mockRepositoryWrapper.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_WhenTransactLinkExists_ShouldReturnMappedDTO()
    {
        // Arrange
        var testEntity = new TransactionLink { StreetcodeId = 1, Url = "test.com" };
        var expectedDto = new TransactLinkDTO { StreetcodeId = 1, Url = "test.com" };
        var request = new GetTransactLinkByStreetcodeIdQuery(testEntity.StreetcodeId);

        _mockTransactLinksRepository
            .Setup(x => x.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<TransactionLink, bool>>>(), null))
            .ReturnsAsync(testEntity);

        _mockMapper.Setup(m => m.Map<TransactLinkDTO>(It.IsAny<TransactionLink>())).Returns(expectedDto);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedDto);
        _mockTransactLinksRepository.Verify(
            x => x.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<TransactionLink, bool>>>(), null),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenTransactLinkDoesNotExistButStreetcodeExists_ShouldReturnNullResult()
    {
        // Arrange
        _mockTransactLinksRepository
            .Setup(x => x.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<TransactionLink, bool>>>(), null))
            .ReturnsAsync((TransactionLink)null);

        // Corrected: Use StreetcodeContent entity
        _mockStreetcodeRepository
            .Setup(x => x.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<StreetcodeContent, bool>>>(), null))
            .ReturnsAsync(new StreetcodeContent { Id = 1 });

        var request = new GetTransactLinkByStreetcodeIdQuery(1);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
        _mockTransactLinksRepository.Verify(
            x => x.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<TransactionLink, bool>>>(), null),
            Times.Once);
        _mockStreetcodeRepository.Verify(
            x => x.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<StreetcodeContent, bool>>>(), null),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenTransactLinkAndStreetcodeDoNotExist_ShouldReturnFailure()
    {
        // Arrange
        _mockTransactLinksRepository
            .Setup(x => x.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<TransactionLink, bool>>>(), null))
            .ReturnsAsync((TransactionLink)null);

        // Corrected: Use StreetcodeContent entity
        _mockStreetcodeRepository
            .Setup(x => x.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<StreetcodeContent, bool>>>(), null))
            .ReturnsAsync((StreetcodeContent)null);

        var request = new GetTransactLinkByStreetcodeIdQuery(999);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);
        var errorMsg = Errors_Common.NotFoundByStreetcode.FormatWith("transaction link", request.StreetcodeId);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().NotBeEmpty();
        result.Errors.First().Message.Should().Be(errorMsg);

        _mockTransactLinksRepository.Verify(
            x => x.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<TransactionLink, bool>>>(), null),
            Times.Once);
        _mockStreetcodeRepository.Verify(
            x => x.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<StreetcodeContent, bool>>>(), null),
            Times.Once);
        _mockLogger.Verify(l => l.LogError(It.IsAny<GetTransactLinkByStreetcodeIdQuery>(), It.IsAny<string>()), Times.Once);
    }
}