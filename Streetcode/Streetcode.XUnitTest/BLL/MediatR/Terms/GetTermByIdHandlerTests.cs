using AutoMapper;
using Moq;
using Streetcode.BLL.DTO.Streetcode.TextContent;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.Term.GetById;
using Streetcode.DAL.Entities.Streetcode.TextContent;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;
using static FluentResults.Result;

namespace Streetcode.XUnitTest.BLL.MediatR.Terms.GetTermById;

public class GetTermByIdHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILoggerService> _mockLogger;

    public GetTermByIdHandlerTests()
    {
        _mockRepository = new Mock<IRepositoryWrapper>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILoggerService>();
    }

    [Fact]
    public async Task Handler_Returns_Term_By_Id_Successfully()
    {
        // Arrange
        var termId = 1;
        var term = new Term { Id = termId, Title = "Test Title", Description = "Test Description" };
        var termDto = new TermDTO { Id = termId, Title = "Test Title", Description = "Test Description" };

        // We use It.IsAny<>() to match the predicate, and we explicitly include 'null' for the second parameter.
        _mockRepository.Setup(r => r.TermRepository.GetFirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Term, bool>>>(), null))
            .ReturnsAsync(term);

        _mockMapper.Setup(m => m.Map<TermDTO>(It.IsAny<Term>())).Returns(termDto);

        var handler = new GetTermByIdHandler(_mockRepository.Object, _mockMapper.Object, _mockLogger.Object);

        // Act
        var result = await handler.Handle(new GetTermByIdQuery(termId), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(termId, result.Value.Id);

        // This Verify call should also be updated to use It.IsAny<>() for consistency.
        _mockRepository.Verify(
            r => r.TermRepository.GetFirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Term, bool>>>(), null), Times.Once);
    }

    [Fact]
    public async Task Handler_Returns_Failure_When_Term_Not_Found()
    {
        // Arrange
        var termId = 99;

        _mockRepository.Setup(r => r.TermRepository.GetFirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Term, bool>>>(), null))
            .ReturnsAsync((Term)null!);

        var handler = new GetTermByIdHandler(_mockRepository.Object, _mockMapper.Object, _mockLogger.Object);

        // Act
        var result = await handler.Handle(new GetTermByIdQuery(termId), CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains($"Cannot find any term with corresponding id: {termId}", result.Errors.First().Message);
        _mockLogger.Verify(l => l.LogError(It.IsAny<object>(), It.IsAny<string>()), Times.Once);
    }
}