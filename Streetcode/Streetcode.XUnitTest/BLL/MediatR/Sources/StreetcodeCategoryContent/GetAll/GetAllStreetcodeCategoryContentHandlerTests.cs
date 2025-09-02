using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Sources;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Sources.StreetcodeCategoryContent.Delete;
using Streetcode.BLL.MediatR.Sources.StreetcodeCategoryContent.GetAll;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.Sources.StreetcodeCategoryContent.GetAll
{
    public class GetAllStreetcodeCategoryContentHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILoggerService> _mockLogger;
        private readonly GetAllStreetcodeCategoryHandler _handler;
        public GetAllStreetcodeCategoryContentHandlerTests()
        {
            _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILoggerService>();
            _handler = new GetAllStreetcodeCategoryHandler(
                _mockLogger.Object,
                _mockMapper.Object,
                _mockRepositoryWrapper.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnAllStreetcodeCategoryContents_WhenSuccess()
        {
            var entities = new List<DAL.Entities.Sources.StreetcodeCategoryContent>
            {
                new DAL.Entities.Sources.StreetcodeCategoryContent
                {
                    Id = 1,
                    StreetcodeId = 1,
                    SourceLinkCategoryId = 2
                },
                new DAL.Entities.Sources.StreetcodeCategoryContent
                {
                    Id = 2,
                    StreetcodeId = 3,
                    SourceLinkCategoryId = 4
                }
            };

            var dtos = new List<StreetcodeCategoryContentDTO>
            {
                new StreetcodeCategoryContentDTO
                {
                    Id = 1,
                    StreetcodeId = 1,
                    SourceLinkCategoryId = 2
                },
                new StreetcodeCategoryContentDTO
                {
                    Id = 2,
                    StreetcodeId = 3,
                    SourceLinkCategoryId = 4
                }
            };
            _mockRepositoryWrapper.Setup(r => r.StreetcodeCategoryContentRepository.GetAllAsync(
                It.IsAny<Expression<Func<DAL.Entities.Sources.StreetcodeCategoryContent, bool>>>(),
                It.IsAny<Func<IQueryable<DAL.Entities.Sources.StreetcodeCategoryContent>, IIncludableQueryable<DAL.Entities.Sources.StreetcodeCategoryContent, object>>>()))
                .ReturnsAsync(entities);

            _mockMapper.Setup(m => m.Map<List<StreetcodeCategoryContentDTO>>(entities)).Returns(dtos);

            var result = await _handler.Handle(new GetAllStreetcodeCategoryContentQuery(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Should().HaveCount(dtos.Count);

            _mockRepositoryWrapper.Verify(
                r => r.StreetcodeCategoryContentRepository.GetAllAsync(
                It.IsAny<Expression<Func<DAL.Entities.Sources.StreetcodeCategoryContent, bool>>>(),
                It.IsAny<Func<IQueryable<DAL.Entities.Sources.StreetcodeCategoryContent>, IIncludableQueryable<DAL.Entities.Sources.StreetcodeCategoryContent, object>>>()),
                Times.Once);

            _mockMapper.Verify(m => m.Map<List<StreetcodeCategoryContentDTO>>(entities), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnOkFail_WhenEntityNotFound()
        {
            var entities = new List<DAL.Entities.Sources.StreetcodeCategoryContent>
            {
                new DAL.Entities.Sources.StreetcodeCategoryContent
                {
                    Id = 1,
                    StreetcodeId = 1,
                    SourceLinkCategoryId = 2
                },
                new DAL.Entities.Sources.StreetcodeCategoryContent
                {
                    Id = 2,
                    StreetcodeId = 3,
                    SourceLinkCategoryId = 4
                }
            };

            var dtos = new List<StreetcodeCategoryContentDTO>
            {
                new StreetcodeCategoryContentDTO
                {
                    Id = 1,
                    StreetcodeId = 1,
                    SourceLinkCategoryId = 2
                },
                new StreetcodeCategoryContentDTO
                {
                    Id = 2,
                    StreetcodeId = 3,
                    SourceLinkCategoryId = 4
                }
            };

            _mockRepositoryWrapper.Setup(r => r.StreetcodeCategoryContentRepository.GetAllAsync(
                It.IsAny<Expression<Func<DAL.Entities.Sources.StreetcodeCategoryContent, bool>>>(),
                It.IsAny<Func<IQueryable<DAL.Entities.Sources.StreetcodeCategoryContent>, IIncludableQueryable<DAL.Entities.Sources.StreetcodeCategoryContent, object>>>()))
                .ReturnsAsync((IEnumerable<DAL.Entities.Sources.StreetcodeCategoryContent>)null);

            var result = await _handler.Handle(new GetAllStreetcodeCategoryContentQuery(), CancellationToken.None);

            result.IsFailed.Should().BeTrue();
            result.Errors.Should().ContainSingle(e =>
                e.Message == "Cannot find any streetcodeCategoryContent");

            _mockRepositoryWrapper.Verify(
                r => r.StreetcodeCategoryContentRepository.GetAllAsync(
                It.IsAny<Expression<Func<DAL.Entities.Sources.StreetcodeCategoryContent, bool>>>(),
                It.IsAny<Func<IQueryable<DAL.Entities.Sources.StreetcodeCategoryContent>, IIncludableQueryable<DAL.Entities.Sources.StreetcodeCategoryContent, object>>>()),
                Times.Once);

            _mockMapper.Verify(m => m.Map<List<StreetcodeCategoryContentDTO>>(entities), Times.Never);
        }
    }
}
