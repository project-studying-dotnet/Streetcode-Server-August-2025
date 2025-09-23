using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Users.ChangePassword;

namespace Streetcode.BLL.MediatR.Users.ChangePassword
{
    public record ChangePasswordCommand(ChangePasswordRequestDto changePasswordRequestDto) : IRequest<Result<ChangePasswordResponseDto>>;
}
