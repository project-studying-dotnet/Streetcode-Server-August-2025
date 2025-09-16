using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Streetcode.BLL.DTO.Users;
using Streetcode.BLL.DTO.Users.Logout;
using Streetcode.BLL.Interfaces.Jwt;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Users.Register;
using Streetcode.BLL.Resources;
using Streetcode.DAL.Entities.Users;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Users.Logout
{
    public class LogoutUserHandler : IRequestHandler<LogoutUserCommand, Result<LogoutResponceDTO>>
    {
        private readonly ILoggerService _loggerService;
        private readonly IJwtTokenService _jwtTokenService;

        public LogoutUserHandler(ILoggerService loggerService, IJwtTokenService jwtTokenService)
        {
            _loggerService = loggerService;
            _jwtTokenService = jwtTokenService;
        }

        public async Task<Result<LogoutResponceDTO>> Handle(LogoutUserCommand request, CancellationToken cancellationToken)
        {
            var result = await _jwtTokenService.RevokeRefreshTokenAsync(request.logoutRequestDTO.RefreshToken);

            if (result.IsFailed)
            {
                string errorMsg = Errors_Jwt.RefreshTokenNotFound;
                _loggerService.LogError(request, errorMsg);
                return Result.Fail(new Error(errorMsg));
            }

            return Result.Ok(new LogoutResponceDTO
            {
                IsSuccess = true,
                Message = "Logout successful."
            });
        }
    }
}
