using AutoMapper;
using FluentAssertions;
using Moq;
using Streetcode.BLL.DTO.Transactions;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Transactions.TransactionLink.GetAll;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.DAL.Entities.Transactions;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Repositories.Interfaces.Transactions;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.Transactions;
public class GetAllTransactLinksHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
    private readonly Mock<ITransactLinksRepository> _mockTransactLinksRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly GetAllTransactLinksHandler _handler;

    public GetAllTransactLinksHandlerTests()
    {
        _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
        _mockTransactLinksRepository = new Mock<ITransactLinksRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILoggerService>();

        // Setup the repository wrapper to return the mocked TransactLinksRepository
        _mockRepositoryWrapper.Setup(r => r.TransactLinksRepository)
            .Returns(_mockTransactLinksRepository.Object);

        _handler = new GetAllTransactLinksHandler(
            _mockRepositoryWrapper.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_WhenTransactLinksExist_ShouldReturnMappedDTOs()
    {
        // Arrange
        var testEntities = new List<TransactionLink>
        {
            new TransactionLink { Id = 1, Url = "test1.com" },
            new TransactionLink { Id = 2, Url = "test2.com" }
        };

        var testDtos = new List<TransactLinkDTO>
        {
            new TransactLinkDTO { Id = 1, Url = "test1.com" },
            new TransactLinkDTO { Id = 2, Url = "test2.com" }
        };

        // Setup mock repository to return the test entities
        _mockTransactLinksRepository.Setup(r => r.GetAllAsync(null, null))
            .ReturnsAsync(testEntities);

        // Setup mock mapper to return the test DTOs
        _mockMapper.Setup(m => m.Map<IEnumerable<TransactLinkDTO>>(It.IsAny<IEnumerable<TransactionLink>>()))
            .Returns(testDtos);

        var query = new GetAllTransactLinksQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(testDtos);

        _mockTransactLinksRepository.Verify(r => r.GetAllAsync(null, null), Times.Once);
        _mockMapper.Verify(m => m.Map<IEnumerable<TransactLinkDTO>>(It.IsAny<IEnumerable<TransactionLink>>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRepositoryReturnsNull_ShouldReturnFailure()
    {
        // Arrange
        _mockTransactLinksRepository.Setup(r => r.GetAllAsync(null, null))
            .ReturnsAsync((IEnumerable<TransactionLink>)null);

        var query = new GetAllTransactLinksQuery();
        var errorMsg = Errors_Common.NotFoundAny.FormatWith("transaction link");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Errors.First().Message.Should().Be(errorMsg);

        _mockTransactLinksRepository.Verify(r => r.GetAllAsync(null, null), Times.Once);
        _mockLogger.Verify(l => l.LogError(It.IsAny<GetAllTransactLinksQuery>(), It.IsAny<string>()), Times.Once);
    }
}