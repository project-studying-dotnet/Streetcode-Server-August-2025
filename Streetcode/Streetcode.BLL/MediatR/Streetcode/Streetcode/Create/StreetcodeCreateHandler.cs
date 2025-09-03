using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.AdditionalContent.Tag;
using Streetcode.BLL.DTO.ArtGallery;
using Streetcode.BLL.DTO.Locations;
using Streetcode.BLL.DTO.Media.Art;
using Streetcode.BLL.DTO.Media.Images;
using Streetcode.BLL.DTO.Streetcode;
using Streetcode.BLL.DTO.Toponyms;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Entities.AdditionalContent;
using Streetcode.DAL.Entities.Analytics;
using Streetcode.DAL.Entities.Media.Images;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Streetcode.Streetcode.Create;

public class StreetcodeCreateHandler : IRequestHandler<StreetcodeCreateCommand, Result<StreetcodeDTO>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;

    public StreetcodeCreateHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, ILoggerService logger)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<StreetcodeDTO>> Handle(StreetcodeCreateCommand request, CancellationToken cancellationToken)
    {
        using (var transactionScope = _repositoryWrapper.BeginTransaction())
        {
            try
            {
                var streetcodeEntity = _mapper.Map<StreetcodeContent>(request.NewStreetcode);

                streetcodeEntity.CreatedAt = streetcodeEntity.UpdatedAt = DateTime.UtcNow;
                streetcodeEntity.ViewCount = 0;

                _repositoryWrapper.StreetcodeRepository.Create(streetcodeEntity);

                var saveResult = await _repositoryWrapper.SaveChangesAsync();
                if (saveResult == 0)
                {
                    return CreateErrorResult<StreetcodeDTO>(request, "Failed to save streetcode to database");
                }

                var imagesDetails = request.NewStreetcode.ImagesDetails;
                if (imagesDetails is null || !imagesDetails.Any())
                {
                    return CreateErrorResult<StreetcodeDTO>(request, "ImagesDetails cannot be empty");
                }

                var imageIds = imagesDetails.Select(x => x.ImageId).Where(id => id > 0).Distinct().ToList();
                if (imageIds.Count == 0)
                {
                    return CreateErrorResult<StreetcodeDTO>(request, "Image IDs cannot be empty");
                }

                await AddImagesAsync(streetcodeEntity, imageIds);

                if (request.NewStreetcode.Tags is null || !request.NewStreetcode.Tags.Any())
                {
                    return CreateErrorResult<StreetcodeDTO>(request, "Tags cannot be empty");
                }

                await AddTags(streetcodeEntity, request.NewStreetcode.Tags);
                await AddImagesDetails(request.NewStreetcode.ImagesDetails);
                await AddToponyms(streetcodeEntity, request.NewStreetcode.Toponyms);

                AddMapPoints(streetcodeEntity, request.NewStreetcode.MapPoints);

                saveResult = await _repositoryWrapper.SaveChangesAsync();

                if (saveResult == 0)
                {
                    return CreateErrorResult<StreetcodeDTO>(request, "Failed to save streetcode to database");
                }

                await AddArtGalleryAsync(streetcodeEntity, request.NewStreetcode.Arts, request.NewStreetcode.StreetcodeArtSlides);

                var resultDto = _mapper.Map<StreetcodeDTO>(streetcodeEntity);

                _logger.LogInformation($"Success! Streetcode with ID {resultDto.Id} was created");

                transactionScope.Complete();
                return Result.Ok(resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(request, ex.Message);
                return Result.Fail<StreetcodeDTO>(ex.Message);
            }
        }
    }

    private Result<T> CreateErrorResult<T>(StreetcodeCreateCommand request, string errorMessage)
        where T : class
    {
        _logger.LogError(request, errorMessage);

        return Result.Fail<T>(new Error(errorMessage));
    }

    private async Task AddImagesDetails(IEnumerable<ImageDetailsDto> imagesDetailsDtos)
    {
        var imageDetails = _mapper.Map<IEnumerable<ImageDetails>>(imagesDetailsDtos);
        await _repositoryWrapper.ImageDetailsRepository.CreateRangeAsync(imageDetails);
    }

    private async Task AddImagesAsync(StreetcodeContent streetcode, IEnumerable<int> imagesIds)
    {
        var streetcodeImages = imagesIds
            .Select(imageId => new StreetcodeImage()
            {
                ImageId = imageId,
                StreetcodeId = streetcode.Id,
            })
            .ToList();

        await _repositoryWrapper.StreetcodeImageRepository.CreateRangeAsync(streetcodeImages);
    }

    private async Task AddTags(StreetcodeContent streetcode, IEnumerable<StreetcodeTagDTO> tags)
    {
        var tagsList = tags.ToList();
        var indexedTags = new List<StreetcodeTagIndex>();

        for (int i = 0; i < tagsList.Count; i++)
        {
            var newTagIndex = new StreetcodeTagIndex
            {
                StreetcodeId = streetcode.Id,
                TagId = tagsList[i].Id,
                IsVisible = tagsList[i].IsVisible,
                Index = i,
            };

            if (tagsList[i].Id <= 0)
            {
                var exists = await _repositoryWrapper.TagRepository.GetFirstOrDefaultAsync(t => tagsList[i].Title == t.Title);
                if (exists is not null)
                {
                    throw new InvalidOperationException("Tag with the same title already exists");
                }

                var newTag = _mapper.Map<Tag>(tagsList[i]);
                newTag.Id = 0;
                newTagIndex.Tag = newTag;
            }

            indexedTags.Add(newTagIndex);
        }

        await _repositoryWrapper.StreetcodeTagIndexRepository.CreateRangeAsync(indexedTags);
    }

    private async Task AddArtGalleryAsync(StreetcodeContent streetcode, IEnumerable<ArtCreateUpdateDTO> arts, IEnumerable<StreetcodeArtSlideCreateUpdateDTO> streetcodeArtSlides)
    {
        var artSlidesList = streetcodeArtSlides?.ToList() ?? new List<StreetcodeArtSlideCreateUpdateDTO>();
        var artsList = arts?.ToList() ?? new List<ArtCreateUpdateDTO>();

        if (artSlidesList.Count == 0 && artsList.Count == 0)
        {
            return;
        }

        // Get the list of Art IDs that are actually used in slides
        var usedArtIds = new HashSet<int>(
            artSlidesList.SelectMany(slide => slide.StreetcodeArts)
                         .Select(streetcodeArt => streetcodeArt.ArtId));

        var filteredArts = artsList.Where(art => usedArtIds.Contains(art.Id)).ToList();

        // Verify that all required ImageIds exist
        var imageIds = filteredArts.Select(a => a.ImageId).Distinct().ToList();
        var existingImages = await _repositoryWrapper.ImageRepository.GetAllAsync(i => imageIds.Contains(i.Id));
        var existingImageIds = new HashSet<int>(existingImages.Select(i => i.Id));

        foreach (var artDto in filteredArts)
        {
            if (!existingImageIds.Contains(artDto.ImageId))
            {
                throw new InvalidOperationException($"Image with ID {artDto.ImageId} does not exist");
            }
        }

        // Create Arts
        var newArts = _mapper.Map<List<Art>>(filteredArts);

        await _repositoryWrapper.ArtRepository.CreateRangeAsync(newArts);
        await _repositoryWrapper.SaveChangesAsync();

        // Create ArtSlides
        var newArtSlides = _mapper.Map<List<StreetcodeArtSlide>>(artSlidesList);
        newArtSlides.ForEach(artSlide => artSlide.StreetcodeId = streetcode.Id);

        await _repositoryWrapper.StreetcodeArtSlideRepository.CreateRangeAsync(newArtSlides);
        await _repositoryWrapper.SaveChangesAsync();

        // Map old Ids => new Ids
        var artIdMap = filteredArts
            .Zip(newArts, (placeholderArt, newArt) => new { PlaceholderId = placeholderArt.Id, RealId = newArt.Id })
            .ToDictionary(x => x.PlaceholderId, x => x.RealId);

        var streetcodeArtEntities = new List<StreetcodeArt>();

        // Create StreetcodeArts
        for (int i = 0; i < artSlidesList.Count; i++)
        {
            var slideDto = artSlidesList[i];
            var slideId = newArtSlides[i].Id;

            foreach (var streetcodeArtDto in slideDto.StreetcodeArts)
            {
                // Ensure that ArtId exists in the map
                if (!artIdMap.TryGetValue(streetcodeArtDto.ArtId, out var newArtId))
                {
                    throw new KeyNotFoundException($"Art ID '{streetcodeArtDto.ArtId}' not found in the mapped arts.");
                }

                var streetcodeArtEntity = _mapper.Map<StreetcodeArt>(streetcodeArtDto);
                streetcodeArtEntity.StreetcodeId = streetcode.Id;
                streetcodeArtEntity.StreetcodeArtSlideId = slideId;
                streetcodeArtEntity.ArtId = newArtId;

                streetcodeArtEntities.Add(streetcodeArtEntity);
            }
        }

        if (streetcodeArtEntities.Count != 0)
        {
            await _repositoryWrapper.StreetcodeArtRepository.CreateRangeAsync(streetcodeArtEntities);
            await _repositoryWrapper.SaveChangesAsync();
        }
    }

    private void AddMapPoints(StreetcodeContent streetcode, IEnumerable<MapPointDTO>? mapPoints)
    {
        if (mapPoints is null || !mapPoints.Any())
        {
            return;
        }

        var mapPointsToCreate = new List<StatisticRecord>();

        foreach (var mapPointDto in mapPoints)
        {
            var newMapPoint = _mapper.Map<StatisticRecord>(mapPointDto);

            var streetcodeCoordinate = streetcode.Coordinates.FirstOrDefault(x =>
                x.Latitude == newMapPoint.StreetcodeCoordinate.Latitude
                && x.Longtitude == newMapPoint.StreetcodeCoordinate.Longtitude);

            if (streetcodeCoordinate is null)
            {
                throw new InvalidOperationException();
            }
            else
            {
                newMapPoint.StreetcodeCoordinate = streetcodeCoordinate;
            }

            mapPointsToCreate.Add(newMapPoint);
        }

        streetcode.StatisticRecords.AddRange(mapPointsToCreate);
    }

    private async Task AddToponyms(StreetcodeContent streetcode, IEnumerable<StreetcodeToponymCreateUpdateDTO>? toponyms)
    {
        if (toponyms is null || !toponyms.Any())
        {
            return;
        }

        var toponymNames = toponyms.Select(x => x.StreetName.Trim()).Distinct().ToList();

        var existingToponyms = await _repositoryWrapper.ToponymRepository
            .GetAllAsync(x => toponymNames.Contains(x.StreetName));

        streetcode.Toponyms.AddRange(existingToponyms);
    }
}