using FluentResults;
using MediatR;

namespace Streetcode.BLL.MediatR.ArtGallery.GetSlidesCountByStreetcodeId;

public record GetSlidesCountByStreetcodeIdQuery(uint StreetcodeId)
    : IRequest<Result<int>>;
