using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.ArtGallery;

namespace Streetcode.BLL.MediatR.ArtGallery;

public record GetArtSlidesByStreetcodeIdQuery(uint StreetcodeId)
    : IRequest<Result<IEnumerable<StreetcodeArtSlideDTO>>>;