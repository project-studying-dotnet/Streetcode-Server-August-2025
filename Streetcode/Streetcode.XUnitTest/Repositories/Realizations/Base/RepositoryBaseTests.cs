using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Streetcode.DAL.Entities.Media;
using Streetcode.DAL.Persistence;
using Streetcode.DAL.Repositories.Realizations.Base;
using Ardalis.Specification;

namespace Streetcode.XUnitTest.Repositories.Realizations.Base
{
    public class RepositoryBaseTests
    {
        public RepositoryBase<Video> GetRepository()
        {
            var options = new DbContextOptionsBuilder<StreetcodeDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var dbContext = new TestDbContext(options);
            dbContext.Videos.AddRange(
                new Video { Id = 1, Url = "https://test1.com" },
                new Video { Id = 2, Url = "https://test2.com" });
            dbContext.SaveChanges();

            return new TestVideoRepository(dbContext);
        }

        [Fact]
        public async Task ListAsync_ReturnsEntities()
        {
            // Arrange
            var repo = GetRepository();
            var spec = new VideoSpecification();

            // Act
            var result = await repo.ListAsync(spec, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetBySpecAsync_ReturnsEntity()
        {
            // Arrange
            var repo = GetRepository();
            var spec = new VideoSpecification();

            // Act
            var result = await repo.GetBySpecAsync(spec, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }

        [Fact]
        public async Task CountAsync_ReturnsCorrectNumber()
        {
            // Arrange
            var repo = GetRepository();
            var spec = new VideoSpecification();

            // Act
            var count = await repo.CountAsync(spec, CancellationToken.None);

            // Assert
            Assert.Equal(2, count);
        }

        [Fact]
        public async Task AnyAsync_ReturnsTrue()
        {
            // Arrange
            var repo = GetRepository();
            var spec = new VideoSpecification();

            // Act
            var exists = await repo.AnyAsync(spec, CancellationToken.None);

            // Assert
            Assert.True(exists);
        }

        private class TestDbContext : StreetcodeDbContext
        {
            public TestDbContext(DbContextOptions<StreetcodeDbContext> options)
                : base(options)
            {
            }
        }

        private class TestVideoRepository : RepositoryBase<Video>
        {
            public TestVideoRepository(StreetcodeDbContext dbContext)
                : base(dbContext)
            {
            }
        }

        private class VideoSpecification : Specification<Video>
        {
            public VideoSpecification()
            {
                Query.Where(v => v.Id > 0);
            }
        }
    }
}
