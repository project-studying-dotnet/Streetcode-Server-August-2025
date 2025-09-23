using FluentResults;
using MediatR;
using Moq;
using Streetcode.BLL.MediatR.Streetcode.Term.Delete;
using Streetcode.DAL.Entities.Streetcode.TextContent;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;
using static FluentResults.Result;

namespace Streetcode.XUnitTest.BLL.MediatR.Terms.DeleteTerm;

public class DeleteTermHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepository;

    public DeleteTermHandlerTests()
    {
        _mockRepository = new Mock<IRepositoryWrapper>();
    }

    [Fact]
    public async Task Handler_Deletes_Term_Successfully()
    {
        // Arrange
        var termId = 1;
        var existingTerm = new Term { Id = termId };

        // Use It.IsAny<>() to match any predicate expression
        _mockRepository.Setup(r => r.TermRepository.GetSingleOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Term, bool>>>(), null))
            .ReturnsAsync(existingTerm);

        _mockRepository.Setup(r => r.TermRepository.Delete(existingTerm));
        _mockRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        var handler = new DeleteTermHandler(_mockRepository.Object);

        // Act
        var result = await handler.Handle(new DeleteTermCommand(termId), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _mockRepository.Verify(r => r.TermRepository.Delete(existingTerm), Times.Once);
        _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handler_Fails_When_Term_Not_Found()
    {
        // Arrange
        var termId = 99;

        _mockRepository.Setup(r => r.TermRepository.GetSingleOrDefaultAsync(
            t => t.Id == termId, null))
            .ReturnsAsync((Term)null!);

        var handler = new DeleteTermHandler(_mockRepository.Object);

        // Act
        var result = await handler.Handle(new DeleteTermCommand(termId), CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains($"Term with id {termId} not found.", result.Errors.First().Message);
        _mockRepository.Verify(r => r.TermRepository.Delete(It.IsAny<Term>()), Times.Never);
        _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }
}