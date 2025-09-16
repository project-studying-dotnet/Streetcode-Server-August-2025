using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using FluentResults;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using Streetcode.BLL.DTO.Users.Logout;
using Streetcode.BLL.Interfaces.Jwt;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Sources.StreetcodeCategoryContent.Create;
using Streetcode.BLL.MediatR.Users.Logout;
using Streetcode.BLL.Resources;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.Users
{
    public class LogoutUserTests
    {
        private readonly Mock<IJwtTokenService> _mockJwtTokenService;
        private readonly Mock<ILoggerService> _mockLoggerService;
        private readonly LogoutUserHandler _handler;
        public LogoutUserTests()
        {
                _mockJwtTokenService = new Mock<IJwtTokenService>();
                _mockLoggerService = new Mock<ILoggerService>();
                _handler = new LogoutUserHandler(_mockLoggerService.Object, _mockJwtTokenService.Object);
        }

        [Fact]
        public async Task Handle_Should_ReturnOkResult_WhenLogoutSuccess()
        {
            var requestDTO = new LogoutRequestDTO
            {
                RefreshToken = "refresh-token"
            };

            var expectedResponse = new LogoutResponceDTO
            {
                IsSuccess = true,
                Message = "Logout successful."
            };

            _mockJwtTokenService.Setup(j => j.RevokeRefreshTokenAsync("refresh-token"))
                .ReturnsAsync(Result.Ok());

            var result = await _handler.Handle(new LogoutUserCommand(requestDTO), CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(expectedResponse.IsSuccess, result.Value.IsSuccess);
            Assert.Equal(expectedResponse.Message, result.Value.Message);

            _mockJwtTokenService.Verify(s => s.RevokeRefreshTokenAsync("refresh-token"), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_WhenRefreshTokenNotFound()
        {
            var requestDTO = new LogoutRequestDTO
            {
                RefreshToken = "refresh-token"
            };

            var expectedResponse = new LogoutResponceDTO
            {
                IsSuccess = true,
                Message = "Logout successful."
            };

            _mockJwtTokenService.Setup(j => j.RevokeRefreshTokenAsync("refresh-token"))
                .ReturnsAsync(Result.Fail(Errors_Jwt.RefreshTokenNotFound));

            var result = await _handler.Handle(new LogoutUserCommand(requestDTO), CancellationToken.None);

            Assert.True(result.IsFailed);
            result.Errors[0].Message.Should().Be(Errors_Jwt.RefreshTokenNotFound);

            _mockJwtTokenService.Verify(s => s.RevokeRefreshTokenAsync("refresh-token"), Times.Once);
        }
    }
}
