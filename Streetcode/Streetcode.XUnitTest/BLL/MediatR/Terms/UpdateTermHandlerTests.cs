using AutoMapper;
using FluentResults;
using MediatR;
using Moq;
using Streetcode.BLL.DTO.Streetcode.TextContent;
using Streetcode.BLL.MediatR.Streetcode.Term.Update;
using Streetcode.DAL.Entities.Streetcode.TextContent;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;
using static FluentResults.Result;

namespace Streetcode.XUnitTest.BLL.MediatR.Terms.UpdateTerm;

public class UpdateTermHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepository;
    private readonly Mock<IMapper> _mockMapper;

    public UpdateTermHandlerTests()
    {
        _mockRepository = new Mock<IRepositoryWrapper>();
        _mockMapper = new Mock<IMapper>();
    }

    [Fact]
    public async Task Handler_Updates_Term_Successfully()
    {
        // Arrange
        var termId = 1;
        var termDto = new CreateTermDTO { Title = "Updated Title", Description = "Updated Description" };
        var existingTerm = new Term { Id = termId, Title = "Original Title", Description = "Original Description" };
        var updatedTermDto = new TermDTO { Id = termId, Title = "Updated Title", Description = "Updated Description" };

        // Mock the repository to return an existing term. Use It.IsAny() for robustness.
        _mockRepository.Setup(r => r.TermRepository.GetSingleOrDefaultAsync(
      It.IsAny<System.Linq.Expressions.Expression<Func<Term, bool>>>(), null))
      .ReturnsAsync(existingTerm);

        // Mock the mapper to handle the update. Use It.IsAny() for both source and destination.
        _mockMapper.Setup(m => m.Map(It.IsAny<CreateTermDTO>(), It.IsAny<Term>()));

        // Mock the repository's update and save methods
        _mockRepository.Setup(r => r.TermRepository.Update(It.IsAny<Term>()));
        _mockRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Mock the mapper to return the final updated DTO
        _mockMapper.Setup(m => m.Map<TermDTO>(It.IsAny<Term>())).Returns(updatedTermDto);

        var handler = new UpdateTermHandler(_mockRepository.Object, _mockMapper.Object);
        var command = new UpdateTermCommand(termId, termDto);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(updatedTermDto.Title, result.Value.Title);
        _mockRepository.Verify(r => r.TermRepository.Update(It.IsAny<Term>()), Times.Once);
        _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handler_Fails_When_Term_Not_Found()
    {
        // Arrange
        var termId = 99;
        var termDto = new CreateTermDTO { Title = "Updated Title", Description = "Updated Description" };

        // Mock the repository to return null, simulating a term not found.
        _mockRepository.Setup(r => r.TermRepository.GetSingleOrDefaultAsync(
      It.IsAny<System.Linq.Expressions.Expression<Func<Term, bool>>>(), null))
      .ReturnsAsync((Term)null!);

        var handler = new UpdateTermHandler(_mockRepository.Object, _mockMapper.Object);
        var command = new UpdateTermCommand(termId, termDto);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains($"Term with id {termId} not found.", result.Errors.First().Message);
        _mockRepository.Verify(r => r.TermRepository.Update(It.IsAny<Term>()), Times.Never);
        _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }
}