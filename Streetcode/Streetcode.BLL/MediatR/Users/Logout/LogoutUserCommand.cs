using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Users;
using Streetcode.BLL.DTO.Users.Logout;

namespace Streetcode.BLL.MediatR.Users.Logout
{
    public record LogoutUserCommand(LogoutRequestDTO logoutRequestDTO) : IRequest<Result<LogoutResponceDTO>>;
}
