using AutoMapper;
using Moq;
using Streetcode.BLL.DTO.Comments;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Comments.Create;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.DAL.Entities.Comments;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.Comments;

public class CreateCommentHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly CreateCommentHandler _handler;

    public CreateCommentHandlerTests()
    {
        _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILoggerService>();
        _handler = new CreateCommentHandler(
            _mockRepositoryWrapper.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var request = CreateValidRequest();
        var commentEntity = CreateCommentEntity();
        var commentDto = CreateCommentDto();

        SetupMocksForSuccess(request, commentEntity, commentDto);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(commentDto, result.Value);
        VerifySuccessCalls(request, commentEntity);
    }

    [Fact]
    public async Task Handle_MapperReturnsNull_ShouldReturnFailure()
    {
        // Arrange
        var request = CreateValidRequest();
        var errorMsg = Errors_Common.CannotMap.FormatWith("CommentCreateDTO");

        _mockMapper.Setup(m => m.Map<CommentContent>(request.NewComment))
            .Returns((CommentContent)null!);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Contains(errorMsg, result.Errors[0].Message);
        _mockLogger.Verify(l => l.LogError(request, errorMsg), Times.Once);
    }

    [Fact]
    public async Task Handle_SaveChangesFails_ShouldReturnFailure()
    {
        // Arrange
        var request = CreateValidRequest();
        var commentEntity = CreateCommentEntity();
        var errorMsg = Errors_Common.FailedToCreate.FormatWith("CommentContent");

        _mockMapper.Setup(m => m.Map<CommentContent>(request.NewComment))
            .Returns(commentEntity);
        _mockRepositoryWrapper.Setup(r => r.CommentRepository.CreateAsync(commentEntity))
            .ReturnsAsync(commentEntity);
        _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(0);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Contains(errorMsg, result.Errors[0].Message);
        _mockLogger.Verify(l => l.LogError(request, errorMsg), Times.Once);
    }

    [Fact]
    public async Task Handle_MapperForDTOReturnsNull_ShouldReturnSuccess()
    {
        // Arrange
        var request = CreateValidRequest();
        var commentEntity = CreateCommentEntity();

        _mockMapper.Setup(m => m.Map<CommentContent>(request.NewComment))
            .Returns(commentEntity);
        _mockRepositoryWrapper.Setup(r => r.CommentRepository.CreateAsync(commentEntity))
            .ReturnsAsync(commentEntity);
        _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);
        _mockMapper.Setup(m => m.Map<CommentDTO>(commentEntity))
            .Returns((CommentDTO)null!);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task Handle_WithParentComment_ShouldReturnSuccess()
    {
        // Arrange
        var request = CreateRequestWithParentComment();
        var commentEntity = CreateCommentEntityWithParent();
        var commentDto = CreateCommentDtoWithParent();

        SetupMocksForSuccess(request, commentEntity, commentDto);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(commentDto, result.Value);
        Assert.Equal(5, result.Value.ParentCommentId);
    }

    [Fact]
    public async Task Handle_WithLongText_ShouldReturnSuccess()
    {
        // Arrange
        var request = CreateRequestWithLongText();
        var commentEntity = CreateCommentEntityWithLongText();
        var commentDto = CreateCommentDtoWithLongText();

        SetupMocksForSuccess(request, commentEntity, commentDto);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(commentDto, result.Value);
        Assert.Equal(1000, result.Value.Text!.Length);
    }

    [Fact]
    public async Task Handle_WithDifferentUserIds_ShouldReturnSuccess()
    {
        // Arrange
        var request = CreateRequestWithDifferentUserId();
        var commentEntity = CreateCommentEntityWithDifferentUserId();
        var commentDto = CreateCommentDtoWithDifferentUserId();

        SetupMocksForSuccess(request, commentEntity, commentDto);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(commentDto, result.Value);
        Assert.Equal(999, result.Value.UserId);
    }

    private static CreateCommentCommand CreateValidRequest()
    {
        return new CreateCommentCommand(new CommentCreateDTO
        {
            Text = "This is a test comment.",
            UserId = 1,
            StreetcodeId = 1,
            ParentCommentId = null
        });
    }

    private static CreateCommentCommand CreateRequestWithParentComment()
    {
        return new CreateCommentCommand(new CommentCreateDTO
        {
            Text = "This is a reply comment.",
            UserId = 2,
            StreetcodeId = 1,
            ParentCommentId = 5
        });
    }

    private static CreateCommentCommand CreateRequestWithLongText()
    {
        return new CreateCommentCommand(new CommentCreateDTO
        {
            Text = new string('a', 1000),
            UserId = 1,
            StreetcodeId = 1,
            ParentCommentId = null
        });
    }

    private static CreateCommentCommand CreateRequestWithDifferentUserId()
    {
        return new CreateCommentCommand(new CommentCreateDTO
        {
            Text = "Comment from different user.",
            UserId = 999,
            StreetcodeId = 1,
            ParentCommentId = null
        });
    }

    private static CommentContent CreateCommentEntity()
    {
        return new CommentContent
        {
            Id = 1,
            Text = "This is a test comment.",
            UserId = 1,
            StreetcodeId = 1,
            ParentCommentId = null
        };
    }

    private static CommentContent CreateCommentEntityWithParent()
    {
        return new CommentContent
        {
            Id = 2,
            Text = "This is a reply comment.",
            UserId = 2,
            StreetcodeId = 1,
            ParentCommentId = 5
        };
    }

    private static CommentContent CreateCommentEntityWithLongText()
    {
        return new CommentContent
        {
            Id = 3,
            Text = new string('a', 1000),
            UserId = 1,
            StreetcodeId = 1,
            ParentCommentId = null
        };
    }

    private static CommentContent CreateCommentEntityWithDifferentUserId()
    {
        return new CommentContent
        {
            Id = 4,
            Text = "Comment from different user.",
            UserId = 999,
            StreetcodeId = 1,
            ParentCommentId = null
        };
    }

    private static CommentDTO CreateCommentDto()
    {
        return new CommentDTO
        {
            Id = 1,
            Text = "This is a test comment.",
            UserId = 1,
            StreetcodeId = 1,
            ParentCommentId = null
        };
    }

    private static CommentDTO CreateCommentDtoWithParent()
    {
        return new CommentDTO
        {
            Id = 2,
            Text = "This is a reply comment.",
            UserId = 2,
            StreetcodeId = 1,
            ParentCommentId = 5
        };
    }

    private static CommentDTO CreateCommentDtoWithLongText()
    {
        return new CommentDTO
        {
            Id = 3,
            Text = new string('a', 1000),
            UserId = 1,
            StreetcodeId = 1,
            ParentCommentId = null
        };
    }

    private static CommentDTO CreateCommentDtoWithDifferentUserId()
    {
        return new CommentDTO
        {
            Id = 4,
            Text = "Comment from different user.",
            UserId = 999,
            StreetcodeId = 1,
            ParentCommentId = null
        };
    }

    private void SetupMocksForSuccess(CreateCommentCommand request, CommentContent entity, CommentDTO dto)
    {
        _mockMapper.Setup(m => m.Map<CommentContent>(request.NewComment))
            .Returns(entity);
        _mockRepositoryWrapper.Setup(r => r.CommentRepository.CreateAsync(entity))
            .ReturnsAsync(entity);
        _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);
        _mockMapper.Setup(m => m.Map<CommentDTO>(entity))
            .Returns(dto);
    }

    private void VerifySuccessCalls(CreateCommentCommand request, CommentContent entity)
    {
        _mockMapper.Verify(m => m.Map<CommentContent>(request.NewComment), Times.Once);
        _mockRepositoryWrapper.Verify(r => r.CommentRepository.CreateAsync(entity), Times.Once);
        _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Once);
        _mockMapper.Verify(m => m.Map<CommentDTO>(entity), Times.Once);
    }
}
