using System.Linq.Expressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Comments;
using Streetcode.BLL.DTO.Users;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Comments.GetByStreetcodeId;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.DAL.Entities.Comments;
using Streetcode.DAL.Entities.Users;
using Streetcode.DAL.Enums;
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
        Assert.Equal(commentDtos.OrderBy(dto => dto.CreatedAt).ToList(), result.Value.ToList());
        _repositoryWrapperMock.Verify(
            repo => repo.CommentRepository.GetCommentTreeByStreetcodeIdAsync(streetcodeId),
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

    [Fact]
    public async Task Handle_NestedComments_ShouldReturnProperHierarchy()
    {
        // Arrange
        int streetcodeId = 1;
        var comments = GetNestedComments();
        var commentDtos = GetNestedCommentDtos();

        SetupRepositoryMocks(comments);
        SetupMapperMocks(comments, commentDtos);

        var request = new GetCommentsByStreetcodeIdQuery(streetcodeId);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var resultList = result.Value.ToList();
        Assert.Single(resultList); // Should have one root comment
        Assert.Equal(2, resultList[0].Replies.Count); // Should have two replies
        Assert.Single(resultList[0].Replies.First().Replies); // First reply should have one nested reply
    }

    [Fact]
    public async Task Handle_CommentsWithUsers_ShouldIncludeUserInfo()
    {
        // Arrange
        int streetcodeId = 1;
        var comments = GetCommentsWithUsers();
        var commentDtos = GetCommentDtosWithUsers();

        SetupRepositoryMocks(comments);
        SetupMapperMocks(comments, commentDtos);

        var request = new GetCommentsByStreetcodeIdQuery(streetcodeId);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var resultList = result.Value.ToList();
        Assert.NotNull(resultList[0].User);
        Assert.Equal("John", resultList[0].User.Name);
        Assert.Equal("Doe", resultList[0].User.Surname);
        Assert.Equal("John Doe", resultList[0].User.DisplayName);
        Assert.Equal("JD", resultList[0].User.Initials);
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

    private static List<CommentContent> GetNestedComments()
    {
        return new List<CommentContent>
        {
            new CommentContent
            {
                Id = 1,
                Text = "Root comment",
                CreatedAt = DateTime.UtcNow.AddMinutes(-15),
                UserId = 1,
                StreetcodeId = 1,
                ParentCommentId = null,
                Replies = new List<CommentContent>
                {
                    new CommentContent
                    {
                        Id = 2,
                        Text = "First reply",
                        CreatedAt = DateTime.UtcNow.AddMinutes(-10),
                        UserId = 2,
                        StreetcodeId = 1,
                        ParentCommentId = 1,
                        Replies = new List<CommentContent>
                        {
                            new CommentContent
                            {
                                Id = 4,
                                Text = "Nested reply",
                                CreatedAt = DateTime.UtcNow.AddMinutes(-5),
                                UserId = 4,
                                StreetcodeId = 1,
                                ParentCommentId = 2,
                                Replies = new List<CommentContent>()
                            }
                        }
                    },
                    new CommentContent
                    {
                        Id = 3,
                        Text = "Second reply",
                        CreatedAt = DateTime.UtcNow.AddMinutes(-8),
                        UserId = 3,
                        StreetcodeId = 1,
                        ParentCommentId = 1,
                        Replies = new List<CommentContent>()
                    }
                }
            }
        };
    }

    private static List<CommentDTO> GetNestedCommentDtos()
    {
        return new List<CommentDTO>
        {
            new CommentDTO
            {
                Id = 1,
                Text = "Root comment",
                CreatedAt = DateTime.UtcNow.AddMinutes(-15),
                UserId = 1,
                StreetcodeId = 1,
                ParentCommentId = null,
                Replies = new List<CommentDTO>
                {
                    new CommentDTO
                    {
                        Id = 2,
                        Text = "First reply",
                        CreatedAt = DateTime.UtcNow.AddMinutes(-10),
                        UserId = 2,
                        StreetcodeId = 1,
                        ParentCommentId = 1,
                        Replies = new List<CommentDTO>
                        {
                            new CommentDTO
                            {
                                Id = 4,
                                Text = "Nested reply",
                                CreatedAt = DateTime.UtcNow.AddMinutes(-5),
                                UserId = 4,
                                StreetcodeId = 1,
                                ParentCommentId = 2,
                                Replies = new List<CommentDTO>()
                            }
                        }
                    },
                    new CommentDTO
                    {
                        Id = 3,
                        Text = "Second reply",
                        CreatedAt = DateTime.UtcNow.AddMinutes(-8),
                        UserId = 3,
                        StreetcodeId = 1,
                        ParentCommentId = 1,
                        Replies = new List<CommentDTO>()
                    }
                }
            }
        };
    }

    private static List<CommentContent> GetCommentsWithUsers()
    {
        var comments = new List<CommentContent>
        {
            new CommentContent
            {
                Id = 1,
                Text = "Comment with user",
                CreatedAt = DateTime.UtcNow,
                UserId = 1,
                StreetcodeId = 1,
                User = new User
                {
                    Id = 1,
                    UserName = "johndoe",
                    Role = UserRole.User,
                },
                Replies = new List<CommentContent>()
            }
        };
        return comments;
    }

    private static List<CommentDTO> GetCommentDtosWithUsers()
    {
        return new List<CommentDTO>
        {
            new CommentDTO
            {
                Id = 1,
                Text = "Comment with user",
                CreatedAt = DateTime.UtcNow,
                UserId = 1,
                StreetcodeId = 1,
                User = new CommentUserDTO
                {
                    Id = 1,
                    Name = "John",
                    Surname = "Doe",
                    UserName = "johndoe",
                    Role = UserRole.User
                },
                Replies = new List<CommentDTO>()
            }
        };
    }

    private void SetupRepositoryMocks(List<CommentContent>? comments)
    {
        _repositoryWrapperMock.Setup(repo => repo.CommentRepository.GetCommentTreeByStreetcodeIdAsync(It.IsAny<int>()))
            .ReturnsAsync(comments);
    }

    private void SetupMapperMocks(List<CommentContent>? comments, List<CommentDTO>? commentDtos)
    {
        _mapperMock.Setup(mapper => mapper.Map<IEnumerable<CommentDTO>>(comments))
            .Returns(commentDtos!);
    }
}