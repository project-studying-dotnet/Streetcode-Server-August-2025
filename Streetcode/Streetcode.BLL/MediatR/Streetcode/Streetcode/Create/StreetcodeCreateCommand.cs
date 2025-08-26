using Streetcode.BLL.DTO.Streetcode.Create;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode;

namespace Streetcode.BLL.MediatR.Streetcode.Streetcode.Create;

public record StreetcodeCreateCommand(StreetcodeCreateDTO newStreetcode) : IRequest<Result<StreetcodeDTO>>;