// <copyright file="ToponymControllerTest.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Streetcode.XUnitTest.Controllers.Toponyms
{
    using AutoFixture;
    using AutoFixture.AutoMoq;
    using FluentResults;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using Streetcode.BLL.DTO.Toponyms;
    using Streetcode.BLL.MediatR.Toponyms.GetAll;
    using Streetcode.BLL.MediatR.Toponyms.GetById;
    using Streetcode.BLL.MediatR.Toponyms.GetByStreetcodeId;
    using Streetcode.WebApi.Controllers.Toponyms;
    using Xunit;

    public class ToponymControllerTest
    {
        private readonly IFixture _fixture;
        private readonly Mock<IMediator> _mediatorMock;
        private readonly ToponymController _sut;

        public ToponymControllerTest()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });
            _mediatorMock = _fixture.Freeze<Mock<IMediator>>();

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IMediator)))
                .Returns(_mediatorMock.Object);

            var httpContext = new DefaultHttpContext
            {
                RequestServices = serviceProviderMock.Object,
            };

            _sut = new ToponymController
            {
                ControllerContext = new ControllerContext
                {
                   HttpContext = httpContext,
                },
            };
        }

        [Fact]
        public async Task GetAll_ReturnsOk_WhenMediatorReturnsSuccess()
        {
            // Arrange
            var allToponymsRequestDTO = _fixture.Create<GetAllToponymsRequestDTO>();
            var allToponymsResponseDTO = _fixture.Create<GetAllToponymsResponseDTO>();

            var resultWrapper = Result.Ok(allToponymsResponseDTO);

            _mediatorMock
                .Setup(x => x.Send(It.IsAny<GetAllToponymsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(resultWrapper);

            // Act
            var actionResult = await _sut.GetAll(allToponymsRequestDTO);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            var value = Assert.IsType<GetAllToponymsResponseDTO>(okResult.Value);
            Assert.Equal(allToponymsResponseDTO, value);
        }

        [Fact]
        public async Task GetAll_ReturnsNotFound_WhenMediatorReturnsSuccessWithNullValue()
        {
            // Arrange
            var allToponymsRequestDTO = _fixture.Create<GetAllToponymsRequestDTO>();

            var resultWrapper = Result.Ok<GetAllToponymsResponseDTO>(default!);

            _mediatorMock
                .Setup(x => x.Send(It.IsAny<GetAllToponymsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(resultWrapper);

            // Act
            var actionResult = await _sut.GetAll(allToponymsRequestDTO);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult);
            Assert.Equal("Found result matching null", notFoundResult.Value);
        }

        [Fact]
        public async Task GetById_ReturnsOk_WhenMediatorReturnsSuccess()
        {
            // Arrange
            var id = _fixture.Create<int>();
            var toponymDTO = _fixture.Create<ToponymDTO>();
            var resultWrapper = Result.Ok(toponymDTO);

            _mediatorMock
                .Setup(x => x.Send(It.IsAny<GetToponymByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(resultWrapper);

            // Act
            var actionResult = await _sut.GetById(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            var value = Assert.IsType<ToponymDTO>(okResult.Value);
            Assert.Equal(toponymDTO, value);

        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenMediatorReturnsSuccessWithNullValue()
        {
            // Arrange
            var id = _fixture.Create<int>();
            var resultWrapper = Result.Ok<GetAllToponymsResponseDTO>(default!);

            _mediatorMock
                .Setup(x => x.Send(It.IsAny<GetAllToponymsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(resultWrapper);

            // Act
            var actionResult = await _sut.GetById(id);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult);
            Assert.Equal("Found result matching null", notFoundResult.Value);
        }

        [Fact]
        public async Task GetByStreetcodeId_ReturnsOk_WhenMediatorReturnsSuccess()
        {
            // Arrange
            var streetCodeId = _fixture.Create<int>();
            var toponymDTO = _fixture.Create<IEnumerable<ToponymDTO>>();
            var resultWrapper = Result.Ok(toponymDTO);

            _mediatorMock
                .Setup(x => x.Send(It.IsAny<GetToponymsByStreetcodeIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(resultWrapper);

            // Act
            var actionResult = await _sut.GetByStreetcodeId(streetCodeId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            var value = Assert.IsAssignableFrom<IEnumerable<ToponymDTO>>(okResult.Value);
            Assert.Equal(toponymDTO, value);
        }

        [Fact]
        public async Task GetByStreetcodeId_ReturnsNotFound_WhenMediatorReturnsSuccessWithNullValue()
        {
            // Arrange
            var streetCodeId = _fixture.Create<int>();
            var resultWrapper = Result.Ok<IEnumerable<ToponymDTO>>(default!);

            _mediatorMock
                .Setup(x => x.Send(It.IsAny<GetToponymsByStreetcodeIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(resultWrapper);

            // Act
            var actionResult = await _sut.GetByStreetcodeId(streetCodeId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult);
            Assert.Equal("Found result matching null", notFoundResult.Value);
        }
    }
}
