using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Users;

namespace Streetcode.BLL.MediatR.Auth.GoogleLogin;

public record GoogleLoginQuery
    : IRequest<Result<LoginResultDTO>>;
