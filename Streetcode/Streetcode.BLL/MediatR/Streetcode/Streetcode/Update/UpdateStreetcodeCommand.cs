using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode;
using Streetcode.BLL.DTO.Streetcode.Update;

namespace Streetcode.BLL.MediatR.Streetcode.Streetcode.Update;

public record UpdateStreetcodeCommand(StreetcodeUpdateDTO Streetcode)
    : IRequest<Result<int>>;
