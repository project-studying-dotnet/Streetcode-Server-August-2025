using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Streetcode.BLL.DTO.News;
using Streetcode.BLL.DTO.Users;
using Streetcode.BLL.MediatR.Newss.Update;
using Streetcode.DAL.Entities.Users;
using Streetcode.DAL.Enums;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Users.Register;

public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, Result<RegisterUserResponseDTO>>
{
    private readonly UserManager<User> _userManager;
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;


    public RegisterUserHandler(UserManager<User> userManager, IMapper mapper, IRepositoryWrapper repositoryWrapper)
    {
        _userManager = userManager;
        _mapper = mapper;
        _repositoryWrapper = repositoryWrapper;
    }

    public async Task<Result<RegisterUserResponseDTO>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var user = _mapper.Map<User>(request.registeredUserDto);

        if (string.IsNullOrWhiteSpace(user.UserName))
        {
            user.UserName = GetUserNameFromEmail(user.Email);
        }

        var uniquenessResult = await EnsureUserDoesNotExistAsync(user, cancellationToken);
        if (uniquenessResult.IsFailed)
        {
            return Result.Fail<RegisterUserResponseDTO>(uniquenessResult.Errors.First().Message);
        }

        user.Role = UserRole.User;
        user.EmailConfirmed = false;

        var createResult = await _userManager.CreateAsync(user, request.registeredUserDto.Password);
        if (!createResult.Succeeded)
        {
            var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
            return Result.Fail<RegisterUserResponseDTO>(errors);
        }

        var response = _mapper.Map<RegisterUserResponseDTO>(user);
        return Result.Ok(response);
    }

    public static string GetUserNameFromEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or empty.", nameof(email));

        var atIndex = email.IndexOf('@');
        if (atIndex <= 0)
            throw new ArgumentException("Invalid email format.", nameof(email));

        return email.Substring(0, atIndex);
    }

    private async Task<Result> EnsureUserDoesNotExistAsync(User user, CancellationToken cancellationToken)
    {
        var existingUser = await _repositoryWrapper.UserRepository
            .GetFirstOrDefaultAsync(
                predicate: u => u.Email == user.Email || u.UserName == user.UserName);

        if (existingUser is not null)
        {
            return Result.Fail("User with this email or username already exists");
        }

        return Result.Ok();
    }
}