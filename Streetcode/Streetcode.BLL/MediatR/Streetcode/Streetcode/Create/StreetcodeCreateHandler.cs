using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Entities.Media.Images;
using Streetcode.BLL.DTO.Media.Images;
using Streetcode.BLL.DTO.AdditionalContent.Tag;
using Streetcode.DAL.Entities.AdditionalContent;

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
                var streetcodeEntity = _mapper.Map<StreetcodeContent>(request.newStreetcode);

                streetcodeEntity.CreatedAt = streetcodeEntity.UpdatedAt = DateTime.UtcNow;
                streetcodeEntity.ViewCount = 0;

                _repositoryWrapper.StreetcodeRepository.Create(streetcodeEntity);

                var saveResult = await _repositoryWrapper.SaveChangesAsync();

                var imageIds = request.newStreetcode.ImagesDetails.Select(x => x.ImageId).ToList();
                if (imageIds == null || !imageIds.Any())
                {
                    return CreateErrorResult<StreetcodeDTO>(request, "Image IDs cannot be empty");
                }

                await AddImagesAsync(streetcodeEntity, imageIds);

                if (request.newStreetcode.Tags == null || !request.newStreetcode.Tags.Any())
                {
                    return CreateErrorResult<StreetcodeDTO>(request, "Tags cannot be empty");
                }

                await AddTags(streetcodeEntity, request.newStreetcode.Tags);
                await _repositoryWrapper.SaveChangesAsync();

                if (request.newStreetcode.ImagesDetails == null || !request.newStreetcode.ImagesDetails.Any())
                {
                    return CreateErrorResult<StreetcodeDTO>(request, "ImagesDetails cannot be empty");
                }

                await AddImagesDetails(request.newStreetcode.ImagesDetails);

                await _repositoryWrapper.SaveChangesAsync();
                if (saveResult == 0)
                {
                    return CreateErrorResult<StreetcodeDTO>(request, "Failed to save streetcode to database");
                }

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
        await _repositoryWrapper.ImageDetailsRepository.CreateRangeAsync(_mapper.Map<IEnumerable<ImageDetails>>(imagesDetailsDtos));
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
}
