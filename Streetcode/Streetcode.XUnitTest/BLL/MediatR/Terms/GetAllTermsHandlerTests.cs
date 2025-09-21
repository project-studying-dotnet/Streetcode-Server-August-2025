using AutoMapper;
using Moq;
using Streetcode.BLL.DTO.Streetcode.TextContent;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.Term.GetAll;
using Streetcode.DAL.Entities.Streetcode.TextContent;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.Terms.GetAllTerm;
public class GetAllTermsHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILoggerService> _mockLogger;

    public GetAllTermsHandlerTests()
    {
        _mockRepository = new Mock<IRepositoryWrapper>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILoggerService>();
    }

    [Fact]
    public async Task Handler_Returns_All_Terms_Successfully()
    {
        // Arrange
        var terms = new List<Term>
        {
            new Term { Id = 1, Title = "Term 1", Description = "Description 1" },
            new Term { Id = 2, Title = "Term 2", Description = "Description 2" }
        };
        var termDtos = new List<TermDTO>
        {
            new TermDTO { Id = 1, Title = "Term 1", Description = "Description 1" },
            new TermDTO { Id = 2, Title = "Term 2", Description = "Description 2" }
        };

        _mockRepository.Setup(r => r.TermRepository.GetAllAsync(null, null)).ReturnsAsync(terms);
        _mockMapper.Setup(m => m.Map<IEnumerable<TermDTO>>(It.IsAny<IEnumerable<Term>>())).Returns(termDtos);

        var handler = new GetAllTermsHandler(_mockRepository.Object, _mockMapper.Object, _mockLogger.Object);

        // Act
        var result = await handler.Handle(new GetAllTermsQuery(), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(termDtos.Count, result.Value.Count());
        _mockRepository.Verify(r => r.TermRepository.GetAllAsync(null, null), Times.Once);
    }

    [Fact]
    public async Task Handler_Returns_Failure_When_No_Terms_Found()
    {
        // Arrange
        _mockRepository.Setup(r => r.TermRepository.GetAllAsync(null, null)).ReturnsAsync((List<Term>)null!);

        var handler = new GetAllTermsHandler(_mockRepository.Object, _mockMapper.Object, _mockLogger.Object);

        // Act
        var result = await handler.Handle(new GetAllTermsQuery(), CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Cannot find any term", result.Errors.First().Message);
        _mockLogger.Verify(l => l.LogError(It.IsAny<object>(), It.IsAny<string>()), Times.Once);
    }
}