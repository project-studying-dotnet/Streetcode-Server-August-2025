using System.Linq.Expressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Comments;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Comments.GetByStreetcodeId;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.DAL.Entities.Comments;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.Comments.GetByStreetcodeId;

public class GetCommentsByStreetcodeIdHandlerTests
{
    private readonly GetCommentsByStreetcodeIdHandler _handler;
    private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
    private readonly Mock<ILoggerService> _loggerMock;
    private readonly Mock<IMapper> _mapperMock;

    public GetCommentsByStreetcodeIdHandlerTests()
    {
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        _loggerMock = new Mock<ILoggerService>();
        _mapperMock = new Mock<IMapper>();

        _handler = new GetCommentsByStreetcodeIdHandler(
            _repositoryWrapperMock.Object,
            _mapperMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_RecordsExist_ShouldReturnOrderedRecords()
    {
        // Arrange
        int streetcodeId = 1;
        var comments = GetComments();
        var commentDtos = GetCommentDtos();

        SetupRepositoryMocks(comments);
        SetupMapperMocks(comments, commentDtos);

        var request = new GetCommentsByStreetcodeIdQuery(streetcodeId);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(commentDtos, result.Value);
        Assert.Equal(commentDtos.OrderBy(dto => dto.CreatedAt).ToList(), result.Value.ToList()); // Перевірка сортування
        _repositoryWrapperMock.Verify(
            repo => repo.CommentRepository.GetAllAsync(
                It.Is<Expression<Func<CommentContent, bool>>>(expr => expr.Compile()(new CommentContent { StreetcodeId = streetcodeId })),
                It.IsAny<Func<IQueryable<CommentContent>, IIncludableQueryable<CommentContent, object>>>()),
            Times.Once());
        _mapperMock.Verify(mapper => mapper.Map<IEnumerable<CommentDTO>>(comments), Times.Once());
        _loggerMock.Verify(logger => logger.LogError(It.IsAny<object>(), It.IsAny<string>()), Times.Never());
    }

    [Fact]
    public async Task Handle_RecordsAreNull_ShouldReturnFail()
    {
        // Arrange
        int streetcodeId = 1;
        string errorMsg = Errors_Common.NotFoundByStreetcode.FormatWith("comment", streetcodeId);

        SetupRepositoryMocks(null);

        var request = new GetCommentsByStreetcodeIdQuery(streetcodeId);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Equal(errorMsg, result.Errors[0].Message);
        _loggerMock.Verify(logger => logger.LogError(request, errorMsg), Times.Once());
        _mapperMock.Verify(mapper => mapper.Map<IEnumerable<CommentDTO>>(It.IsAny<IEnumerable<CommentContent>>()), Times.Never());
    }

    private static List<CommentContent> GetComments()
    {
        return new List<CommentContent>
        {
            new CommentContent
            {
                Id = 1,
                Text = "First comment",
                CreatedAt = DateTime.UtcNow.AddMinutes(-10),
                UpdatedAt = null,
                UserId = 1,
                StreetcodeId = 1,
                ParentCommentId = null,
                Replies = new List<CommentContent>
                {
                    new CommentContent
                    {
                        Id = 2,
                        Text = "Reply to first comment",
                        CreatedAt = DateTime.UtcNow.AddMinutes(-5),
                        UpdatedAt = null,
                        UserId = 2,
                        StreetcodeId = 1,
                        ParentCommentId = 1,
                        Replies = new List<CommentContent>()
                    }
                }
            },
            new CommentContent
            {
                Id = 3,
                Text = "Second comment",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null,
                UserId = 3,
                StreetcodeId = 1,
                ParentCommentId = null,
                Replies = new List<CommentContent>()
            }
        };
    }

    private static List<CommentDTO> GetCommentDtos()
    {
        return new List<CommentDTO>
        {
            new CommentDTO
            {
                Id = 1,
                Text = "First comment",
                CreatedAt = DateTime.UtcNow.AddMinutes(-10),
                UpdatedAt = null,
                UserId = 1,
                StreetcodeId = 1,
                ParentCommentId = null,
                Replies = new List<CommentDTO>
                {
                    new CommentDTO
                    {
                        Id = 2,
                        Text = "Reply to first comment",
                        CreatedAt = DateTime.UtcNow.AddMinutes(-5),
                        UpdatedAt = null,
                        UserId = 2,
                        StreetcodeId = 1,
                        ParentCommentId = 1,
                        Replies = new List<CommentDTO>()
                    }
                }
            },
            new CommentDTO
            {
                Id = 3,
                Text = "Second comment",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null,
                UserId = 3,
                StreetcodeId = 1,
                ParentCommentId = null,
                Replies = new List<CommentDTO>()
            }
        };
    }

    private void SetupRepositoryMocks(List<CommentContent>? comments)
    {
        _repositoryWrapperMock.Setup(repo => repo.CommentRepository.GetAllAsync(
            It.IsAny<Expression<Func<CommentContent, bool>>>(),
            It.IsAny<Func<IQueryable<CommentContent>, IIncludableQueryable<CommentContent, object>>>()))
            .ReturnsAsync(comments!);
    }

    private void SetupMapperMocks(List<CommentContent>? comments, List<CommentDTO>? commentDtos)
    {
        _mapperMock.Setup(mapper => mapper.Map<IEnumerable<CommentDTO>>(comments))
            .Returns(commentDtos!);
    }
}