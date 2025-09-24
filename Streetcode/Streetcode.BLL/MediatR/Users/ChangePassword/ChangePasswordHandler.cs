using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Streetcode.BLL.DTO.Feedback;
using Streetcode.BLL.DTO.Users.ChangePassword;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Resources;
using Streetcode.DAL.Entities.Users;

namespace Streetcode.BLL.MediatR.Users.ChangePassword
{
    public class ChangePasswordHandler : IRequestHandler<ChangePasswordCommand, Result<ChangePasswordResponseDto>>
    {
        private readonly UserManager<User> _userManager;
        private readonly ILoggerService _loggerService;

        public ChangePasswordHandler(UserManager<User> userManager, ILoggerService loggerService)
        {
            _userManager = userManager;
            _loggerService = loggerService;
        }

        public async Task<Result<ChangePasswordResponseDto>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            var user = _userManager.Users.FirstOrDefault(u => u.Email == request.changePasswordRequestDto.Email);
            if (user == null)
            {
                string errorMsg = "User not found";
                _loggerService.LogError(request, errorMsg);
                return Result.Fail(errorMsg);
            }

            var result = await _userManager.ChangePasswordAsync(user, request.changePasswordRequestDto.OldPassword, request.changePasswordRequestDto.NewPassword);

            if (!result.Succeeded)
            {
                string errorMsg = "Error! Can`t change password";
                _loggerService.LogError(request, errorMsg);
                return Result.Fail(errorMsg);
            }

            return new ChangePasswordResponseDto
            {
                IsSuccess = true,
                Message = "Password changed successfully"
            };
        }
    }
}
