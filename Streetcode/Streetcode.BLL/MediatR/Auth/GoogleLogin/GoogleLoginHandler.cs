using FluentResults;
using MediatR;
using Streetcode.BLL.Interfaces.Jwt;
using Microsoft.AspNetCore.Identity;
using Streetcode.BLL.DTO.Users;
using Streetcode.BLL.Interfaces.Google;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Entities.Users;
using Streetcode.DAL.Enums;

namespace Streetcode.BLL.MediatR.Auth.GoogleLogin;

public class GoogleLoginHandler : IRequestHandler<GoogleLoginQuery, Result<LoginResultDTO>>
{
    private const string Provider = "Google";

    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILoggerService _logger;
    private readonly IGoogleService _googleService;
    private readonly UserManager<User> _userManager;

    public GoogleLoginHandler(
        IJwtTokenService jwtTokenService,
        ILoggerService logger,
        UserManager<User> userManager,
        IGoogleService googleService)
    {
        _jwtTokenService = jwtTokenService;
        _logger = logger;
        _userManager = userManager;
        _googleService = googleService;
    }

    public async Task<Result<LoginResultDTO>> Handle(GoogleLoginQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var userInfo = await _googleService.GetGoogleUserInfoAsync();

            var user = await _userManager.FindByEmailAsync(userInfo.Email);
            if (user == null)
            {
                user = new User
                {
                    Email = userInfo.Email,
                    Name = userInfo.GivenName,
                    Surname = userInfo.FamilyName,
                    UserName = userInfo.Email.Split("@")[0].ToLower()
                };

                var registerResponse = await RegisterUserAsync(request, user, userInfo.Subject);

                if (registerResponse.IsFailed)
                {
                    return Result.Fail(registerResponse.Errors);
                }
            }
            else
            {
                bool isUpdated = false;

                if (user.Name != userInfo.GivenName)
                {
                    user.Name = userInfo.GivenName;
                    isUpdated = true;
                }

                if (user.Surname != userInfo.FamilyName)
                {
                    user.Surname = userInfo.FamilyName;
                    isUpdated = true;
                }

                if (isUpdated)
                {
                    var updateResult = await _userManager.UpdateAsync(user);
                    if (!updateResult.Succeeded)
                    {
                        string errorMessage =
                            updateResult.Errors.FirstOrDefault()?.Description ?? "Error update user";
                        _logger.LogError(request, errorMessage);
                        return Result.Fail(errorMessage);
                    }
                }
            }

            var response = await _jwtTokenService.GenerateTokenAsync(user.Id);

            return Result.Ok(response.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(request, ex.Message);
            return Result.Fail($"Error Authentication Google: {ex.Message}");
        }
    }

    private async Task<Result> RegisterUserAsync(
        GoogleLoginQuery request,
        User user,
        string providerKey)
    {
        try
        {
            var createResult = await _userManager.CreateAsync(user);
            if (createResult.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, nameof(UserRole.User));

                var loginInfo = new UserLoginInfo(Provider, providerKey, Provider);
                var loginResult = await _userManager.AddLoginAsync(user, loginInfo);

                if (!loginResult.Succeeded)
                {
                    string errorMessage = loginResult.Errors.FirstOrDefault()?.Description ??
                                          "Error adding external login";
                    _logger.LogError(request, errorMessage);
                    return Result.Fail(errorMessage);
                }
            }
            else
            {
                string errorMessage = createResult.Errors.FirstOrDefault()?.Description ?? "Error creating user";
                _logger.LogError(request, errorMessage);
                return Result.Fail(errorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(request, ex.Message);
            return Result.Fail(ex.Message);
        }

        return Result.Ok();
    }
}