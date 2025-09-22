using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Streetcode.DAL.Entities.Comments;
using Streetcode.DAL.Entities.Users;
using Streetcode.DAL.Persistence;
using Streetcode.DAL.Repositories.Realizations.Comments;
using Xunit;

namespace Streetcode.XUnitTest.Repositories.Realizations.Comments
{
    public class CommentRepositoryTests
    {
        [Fact]
        public async Task GetCommentTreeByStreetcodeIdAsync_NoComments_ReturnsEmptyList()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext(nameof(GetCommentTreeByStreetcodeIdAsync_NoComments_ReturnsEmptyList));
            var repository = new CommentRepository(dbContext);

            // Act
            var result = await repository.GetCommentTreeByStreetcodeIdAsync(1);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetCommentTreeByStreetcodeIdAsync_OnlyRootComments_ReturnsOrderedRootComments()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext(nameof(GetCommentTreeByStreetcodeIdAsync_OnlyRootComments_ReturnsOrderedRootComments));
            var user = new User { Id = 1 };
            dbContext.Users.Add(user);
            dbContext.Comments.AddRange(
                new CommentContent { Id = 1, StreetcodeId = 1, UserId = 1, User = user, Text = "Root 1", CreatedAt = DateTime.Now.AddHours(-2), IsDeleted = false },
                new CommentContent { Id = 2, StreetcodeId = 1, UserId = 1, User = user, Text = "Root 2", CreatedAt = DateTime.Now.AddHours(-1), IsDeleted = false });
            dbContext.SaveChanges();
            var repository = new CommentRepository(dbContext);

            // Act
            var result = (await repository.GetCommentTreeByStreetcodeIdAsync(1)).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(1, result[0].Id); // Earlier comment should be first
            Assert.Equal(2, result[1].Id);
            Assert.Empty(result[0].Replies);
            Assert.Empty(result[1].Replies);
        }

        [Fact]
        public async Task GetCommentTreeByStreetcodeIdAsync_WithNestedComments_ReturnsCorrectHierarchy()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext(nameof(GetCommentTreeByStreetcodeIdAsync_WithNestedComments_ReturnsCorrectHierarchy));
            var user = new User { Id = 1 };
            dbContext.Users.Add(user);
            dbContext.Comments.AddRange(
                new CommentContent { Id = 1, StreetcodeId = 1, UserId = 1, User = user, Text = "Root", CreatedAt = DateTime.Now.AddHours(-3), IsDeleted = false },
                new CommentContent { Id = 2, StreetcodeId = 1, UserId = 1, User = user, ParentCommentId = 1, Text = "Reply 1", CreatedAt = DateTime.Now.AddHours(-2), IsDeleted = false },
                new CommentContent { Id = 3, StreetcodeId = 1, UserId = 1, User = user, ParentCommentId = 2, Text = "Reply 2", CreatedAt = DateTime.Now.AddHours(-1), IsDeleted = false });
            dbContext.SaveChanges();
            var repository = new CommentRepository(dbContext);

            // Act
            var result = (await repository.GetCommentTreeByStreetcodeIdAsync(1)).ToList();

            // Assert
            Assert.Single(result); // Only one root comment
            Assert.Equal(1, result[0].Id);
            Assert.Single(result[0].Replies); // One direct reply
            Assert.Equal(2, result[0].Replies.First().Id);
            Assert.Single(result[0].Replies.First().Replies); // One nested reply
            Assert.Equal(3, result[0].Replies.First().Replies.First().Id);
        }

        [Fact]
        public async Task GetCommentTreeByStreetcodeIdAsync_DeletedComments_AreExcluded()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext(nameof(GetCommentTreeByStreetcodeIdAsync_DeletedComments_AreExcluded));
            var user = new User { Id = 1 };
            dbContext.Users.Add(user);
            dbContext.Comments.AddRange(
                new CommentContent { Id = 1, StreetcodeId = 1, UserId = 1, User = user, Text = "Active", CreatedAt = DateTime.Now.AddHours(-2), IsDeleted = false },
                new CommentContent { Id = 2, StreetcodeId = 1, UserId = 1, User = user, Text = "Deleted", CreatedAt = DateTime.Now.AddHours(-1), IsDeleted = true });
            dbContext.SaveChanges();
            var repository = new CommentRepository(dbContext);

            // Act
            var result = (await repository.GetCommentTreeByStreetcodeIdAsync(1)).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal(1, result[0].Id);
            Assert.False(result[0].IsDeleted);
        }

        [Fact]
        public async Task GetCommentTreeByStreetcodeIdAsync_DifferentStreetcodeId_ReturnsOnlyMatchingComments()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext(nameof(GetCommentTreeByStreetcodeIdAsync_DifferentStreetcodeId_ReturnsOnlyMatchingComments));
            var user = new User { Id = 1 };
            dbContext.Users.Add(user);
            dbContext.Comments.AddRange(
                new CommentContent { Id = 1, StreetcodeId = 1, UserId = 1, User = user, Text = "Streetcode 1", CreatedAt = DateTime.Now.AddHours(-2), IsDeleted = false },
                new CommentContent { Id = 2, StreetcodeId = 2, UserId = 1, User = user, Text = "Streetcode 2", CreatedAt = DateTime.Now.AddHours(-1), IsDeleted = false });
            dbContext.SaveChanges();
            var repository = new CommentRepository(dbContext);

            // Act
            var result = (await repository.GetCommentTreeByStreetcodeIdAsync(1)).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal(1, result[0].Id);
            Assert.Equal(1, result[0].StreetcodeId);
        }

        private StreetcodeDbContext GetInMemoryDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<StreetcodeDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new StreetcodeDbContext(options);
        }
    }
}
