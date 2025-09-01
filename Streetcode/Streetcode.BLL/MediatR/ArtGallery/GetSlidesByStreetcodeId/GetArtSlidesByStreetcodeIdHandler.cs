using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Streetcode.BLL.DTO.ArtGallery;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.ArtGallery.GetSlidesByStreetcodeId;

public class GetArtSlidesByStreetcodeIdHandler : IRequestHandler<GetArtSlidesByStreetcodeIdQuery, Result<IEnumerable<StreetcodeArtSlideDTO>>>
{
    private readonly IBlobService _blobService;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly IMapper _mapper;

    public GetArtSlidesByStreetcodeIdHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, IBlobService blobService)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
        _blobService = blobService;
    }

    public async Task<Result<IEnumerable<StreetcodeArtSlideDTO>>> Handle(GetArtSlidesByStreetcodeIdQuery request, CancellationToken cancellationToken)
    {
        var slides = await _repositoryWrapper.StreetcodeArtSlideRepository
            .GetAllAsync(
            predicate: s => s.StreetcodeId == request.StreetcodeId,
            include: artSlide => artSlide
                .Include(artSlide => artSlide.StreetcodeArts!)
                .ThenInclude(streetcodeArt => streetcodeArt.Art!)
                .ThenInclude(art => art.Image!));

        var slideDtos = _mapper.Map<IEnumerable<StreetcodeArtSlideDTO>>(slides);
        slideDtos = SetBase64ToImages(slideDtos);

        return Result.Ok(slideDtos);
    }

    private IEnumerable<StreetcodeArtSlideDTO> SetBase64ToImages(IEnumerable<StreetcodeArtSlideDTO> slideDtos)
    {
        foreach (var slide in slideDtos)
        {
            foreach (var art in slide.StreetcodeArts!)
            {
                if (art.Art?.Image is not null && art.Art.Image.BlobName is not null)
                {
                    art.Art.Image.Base64 = _blobService.FindFileInStorageAsBase64(art.Art.Image.BlobName!);
                }
            }
        }

        return slideDtos;
    }
}
