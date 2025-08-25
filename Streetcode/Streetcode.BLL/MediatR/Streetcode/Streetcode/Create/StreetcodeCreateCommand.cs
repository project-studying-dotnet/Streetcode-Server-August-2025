using Streetcode.BLL.DTO.Streetcode.Create;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Partners;
using Streetcode.BLL.DTO.Streetcode;
using Streetcode.BLL.DTO.Streetcode.Create;

namespace Streetcode.BLL.MediatR.Streetcode.Streetcode.Create;

public record StreetcodeCreateCommand(StreetcodeCreateDTO newStreetcode) : IRequest<Result<StreetcodeDTO>>;
