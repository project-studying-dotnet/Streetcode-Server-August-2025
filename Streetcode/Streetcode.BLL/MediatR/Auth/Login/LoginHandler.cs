using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Streetcode.BLL.DTO.Users;
using Streetcode.BLL.Interfaces.Jwt;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.DAL.Entities.Users;

namespace Streetcode.BLL.MediatR.Auth.Login;

public class LoginHandler : IRequestHandler<LoginCommand, Result<LoginResultDTO>>
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILoggerService _logger;
    private readonly UserManager<User> _userManager;

    public LoginHandler(
        UserManager<User> userManager,
        IJwtTokenService jwtTokenService,
        ILoggerService logger)
    {
        _jwtTokenService = jwtTokenService;
        _logger = logger;
        _userManager = userManager;
    }

    public async Task<Result<LoginResultDTO>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(request.userLoginDTO.Login);

            if (user == null || !await _userManager.CheckPasswordAsync(user, request.userLoginDTO.Password))
            {
                string errorMsg = Errors_Auth.IncorrectEmailOrPassword.FormatWith("Login", request.userLoginDTO);
                _logger.LogError(request, errorMsg);
                return Result.Fail(new Error(errorMsg));
            }

            var response = await _jwtTokenService.GenerateTokenAsync(user.Id);

            return Result.Ok(response.Value);
        }
        catch (Exception e)
        {
            _logger.LogError(request, e.Message);
            return Result.Fail(e.Message);
        }
    }
}
