using System.Linq.Expressions;
using FluentAssertions;
using Moq;
using Streetcode.BLL.MediatR.ArtGallery.GetSlidesCountByStreetcodeId;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Repositories.Interfaces.Media.Images;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.ArtGallery.GetSlidesCountByStreetcodeId
{
    public class GetSlidesCountByStreetcodeIdHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
        private readonly Mock<IStreetcodeArtSlideRepository> _mockStreetcodeArtSlideRepository;
        private readonly GetSlidesCountByStreetcodeIdHandler _handler;

        public GetSlidesCountByStreetcodeIdHandlerTests()
        {
            _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
            _mockStreetcodeArtSlideRepository = new Mock<IStreetcodeArtSlideRepository>();
            _mockRepositoryWrapper.Setup(r => r.StreetcodeArtSlideRepository)
                .Returns(_mockStreetcodeArtSlideRepository.Object);
            _handler = new GetSlidesCountByStreetcodeIdHandler(_mockRepositoryWrapper.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnCorrectCount_WhenSlidesExist()
        {
            // Arrange
            uint streetcodeId = 5;
            var slides = PrepareSlides((int)streetcodeId, 3);
            _mockStreetcodeArtSlideRepository.Setup(r => r.FindAll(It.IsAny<System.Linq.Expressions.Expression<System.Func<StreetcodeArtSlide, bool>>>()))
                .Returns(slides.AsQueryable());
            var request = new GetSlidesCountByStreetcodeIdQuerry(streetcodeId);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(3);
        }

        [Fact]
        public async Task Handle_ShouldReturnZero_WhenNoSlidesExist()
        {
            // Arrange
            uint streetcodeId = 10;
            var slides = PrepareSlides((int)streetcodeId, 0);
            _mockStreetcodeArtSlideRepository.Setup(r => r.FindAll(It.IsAny<System.Linq.Expressions.Expression<System.Func<StreetcodeArtSlide, bool>>>()))
                .Returns(slides.AsQueryable());
            var request = new GetSlidesCountByStreetcodeIdQuerry(streetcodeId);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(0);
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenRepositoryReturnsNull()
        {
            // Arrange
            uint streetcodeId = 15;
            _mockStreetcodeArtSlideRepository.Setup(r => r.FindAll(It.IsAny<Expression<Func<StreetcodeArtSlide, bool>>>()))
                .Returns((IQueryable<StreetcodeArtSlide>)null!);
            var request = new GetSlidesCountByStreetcodeIdQuerry(streetcodeId);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await _handler.Handle(request, CancellationToken.None);
            });
        }

        private List<StreetcodeArtSlide> PrepareSlides(int streetcodeId, int count)
        {
            var slides = new List<StreetcodeArtSlide>();
            for (int i = 0; i < count; i++)
            {
                slides.Add(new StreetcodeArtSlide { Id = i + 1, StreetcodeId = streetcodeId });
            }

            return slides;
        }
    }
}
