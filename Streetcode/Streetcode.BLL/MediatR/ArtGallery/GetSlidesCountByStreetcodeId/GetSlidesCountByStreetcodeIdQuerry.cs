using FluentResults;
using MediatR;

namespace Streetcode.BLL.MediatR.ArtGallery.GetSlidesCountByStreetcodeId;

public record GetSlidesCountByStreetcodeIdQuerry(uint StreetcodeId)
    : IRequest<Result<int>>;
