using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Users;

namespace Streetcode.BLL.MediatR.Auth.Login;

public record LoginCommand(UserLoginDTO userLoginDTO)
    : IRequest<Result<LoginResultDTO>>;
