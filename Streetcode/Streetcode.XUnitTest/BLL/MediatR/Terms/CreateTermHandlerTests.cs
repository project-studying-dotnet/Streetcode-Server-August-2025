using AutoMapper;
using Moq;
using Streetcode.BLL.DTO.Streetcode.TextContent;
using Streetcode.BLL.MediatR.Streetcode.Term.Create;
using Streetcode.DAL.Entities.Streetcode.TextContent;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.Terms.CreateTerm;

public class CreateTermHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepository;
    private readonly Mock<IMapper> _mockMapper;

    public CreateTermHandlerTests()
    {
        // Set up mock objects for dependencies
        _mockRepository = new Mock<IRepositoryWrapper>();
        _mockMapper = new Mock<IMapper>();
    }

    [Fact]
    public async Task Handler_Creates_Term_Successfully()
    {
        // Arrange
        // Create dummy data for the test
        var termDto = new CreateTermDTO { Title = "Test Title", Description = "Test Description" };
        var term = new Term { Id = 1, Title = "Test Title", Description = "Test Description" };
        var createdTermDto = new TermDTO { Id = 1, Title = "Test Title", Description = "Test Description" };

        // Configure the mock behavior
        _mockMapper.Setup(m => m.Map<Term>(It.IsAny<CreateTermDTO>())).Returns(term);
        _mockRepository.Setup(r => r.TermRepository.Create(It.IsAny<Term>()));
        _mockRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
        _mockMapper.Setup(m => m.Map<TermDTO>(It.IsAny<Term>())).Returns(createdTermDto);

        var handler = new CreateTermHandler(_mockRepository.Object, _mockMapper.Object);

        // Act
        var result = await handler.Handle(new CreateTermCommand(termDto), CancellationToken.None);

        // Assert
        // Verify the result is successful and the correct methods were called
        Assert.True(result.IsSuccess);
        Assert.Equal(createdTermDto.Id, result.Value.Id);
        _mockRepository.Verify(r => r.TermRepository.Create(It.IsAny<Term>()), Times.Once);
        _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}