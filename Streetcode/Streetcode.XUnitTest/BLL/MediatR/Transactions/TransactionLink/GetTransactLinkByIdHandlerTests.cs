using AutoMapper;
using FluentAssertions;
using Moq;
using Streetcode.BLL.DTO.Transactions;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Transactions.TransactionLink.GetById;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.DAL.Entities.Transactions;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Repositories.Interfaces.Transactions;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.Transactions;
public class GetTransactLinkByIdHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
    private readonly Mock<ITransactLinksRepository> _mockTransactLinksRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly GetTransactLinkByIdHandler _handler;

    public GetTransactLinkByIdHandlerTests()
    {
        _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
        _mockTransactLinksRepository = new Mock<ITransactLinksRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILoggerService>();

        _mockRepositoryWrapper.Setup(r => r.TransactLinksRepository)
            .Returns(_mockTransactLinksRepository.Object);

        _handler = new GetTransactLinkByIdHandler(
            _mockRepositoryWrapper.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_WhenTransactLinkExists_ShouldReturnMappedDTO()
    {
        // Arrange
        var testEntity = new TransactionLink { Id = 1, Url = "test.com" };
        var expectedDto = new TransactLinkDTO { Id = 1, Url = "test.com" };
        var request = new GetTransactLinkByIdQuery(testEntity.Id);

        _mockTransactLinksRepository
            .Setup(x => x.GetFirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<TransactionLink, bool>>>(), null))
            .ReturnsAsync(testEntity);

        _mockMapper.Setup(m => m.Map<TransactLinkDTO>(It.IsAny<TransactionLink>())).Returns(expectedDto);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedDto);
        _mockTransactLinksRepository.Verify(
            x => x.GetFirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<TransactionLink, bool>>>(), null),
            Times.Once);
        _mockMapper.Verify(m => m.Map<TransactLinkDTO>(It.IsAny<TransactionLink>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenTransactLinkDoesNotExist_ShouldReturnFailure()
    {
        // Arrange
        _mockTransactLinksRepository
            .Setup(x => x.GetFirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<TransactionLink, bool>>>(), null))
            .ReturnsAsync((TransactionLink)null);

        var request = new GetTransactLinkByIdQuery(999);
        var errorMsg = Errors_Common.NotFoundById.FormatWith("transaction link", request.Id);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().NotBeEmpty();
        result.Errors.First().Message.Should().Be(errorMsg);
        _mockLogger.Verify(l => l.LogError(It.IsAny<GetTransactLinkByIdQuery>(), It.IsAny<string>()), Times.Once);
    }
}
