using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.AdditionalContent.Tag;
using Streetcode.BLL.DTO.ArtGallery;
using Streetcode.BLL.DTO.Interfaces;
using Streetcode.BLL.DTO.Media.Art;
using Streetcode.BLL.DTO.Media.Images;
using Streetcode.BLL.Enums;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Entities.AdditionalContent;
using Streetcode.DAL.Entities.Media.Images;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Streetcode.Streetcode.Update
{
    public class UpdateStreetcodeHandler : IRequestHandler<UpdateStreetcodeCommand, Result<int>>
    {
        private IRepositoryWrapper _repositoryWrapper;
        private IMapper _mapper;
        private ILoggerService _logger;

        public UpdateStreetcodeHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, ILoggerService logger)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<int>> Handle(UpdateStreetcodeCommand request, CancellationToken cancellationToken)
        {
            using (var transactionScope = _repositoryWrapper.BeginTransaction())
            {
                try
                {
                    var existingEntity = await _repositoryWrapper.StreetcodeRepository.GetFirstOrDefaultAsync(
                        predicate: st => st.Id == request.Streetcode.Id);

                    if (existingEntity is null)
                    {
                        string errorMsg = $"Cannot find any streetcode with corresponding id: {request.Streetcode.Id}";
                        _logger.LogError(request, errorMsg);
                        return Result.Fail(new Error(errorMsg));
                    }

                    _mapper.Map(request.Streetcode, existingEntity);

                    existingEntity.UpdatedAt = DateTime.UtcNow;
                    await UpdateTags(request.Streetcode.Tags);
                    await UpdateImagesAsync(request.Streetcode.Images);
                    await UpdateArtGalleryAsync(existingEntity.Id, request.Streetcode.Arts, request.Streetcode.StreetcodeArtSlides);

                    _repositoryWrapper.StreetcodeRepository.Update(existingEntity);

                    var saveResult = await _repositoryWrapper.SaveChangesAsync();

                    if (saveResult < 0)
                    {
                        const string errorMsg = "Failed to update streetcode in database";
                        _logger.LogError(request, errorMsg);
                        return Result.Fail<int>(errorMsg);
                    }

                    if (saveResult == 0)
                    {
                        _logger.LogInformation($"No changes detected for Streetcode ID {existingEntity.Id}; committing transaction.");
                    }

                    transactionScope.Complete();

                    _logger.LogInformation($"Success! Streetcode with ID {existingEntity.Id} was updated");
                    return Result.Ok(existingEntity.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(request, ex.Message);
                    return Result.Fail<int>(ex.Message);
                }
            }
        }

        private async Task UpdateTags(IEnumerable<StreetcodeTagUpdateDTO>? tags)
        {
            if (tags is null)
            {
                return;
            }

            var (toUpdate, toCreate, toDelete) = CategorizeItems(tags);

            foreach (var newTag in toCreate)
            {
                var existingTag = await _repositoryWrapper.TagRepository.GetFirstOrDefaultAsync(t => t.Title == newTag.Title);
                if (existingTag is not null && existingTag.Id != newTag.Id)
                {
                    throw new InvalidOperationException("Tag titles must be unique.");
                }
            }

            await _repositoryWrapper.StreetcodeTagIndexRepository.CreateRangeAsync(_mapper.Map<IEnumerable<StreetcodeTagIndex>>(toCreate));
            _repositoryWrapper.StreetcodeTagIndexRepository.DeleteRange(_mapper.Map<IEnumerable<StreetcodeTagIndex>>(toDelete));
            _repositoryWrapper.StreetcodeTagIndexRepository.UpdateRange(_mapper.Map<IEnumerable<StreetcodeTagIndex>>(toUpdate));
        }

        private async Task UpdateImagesAsync(IEnumerable<ImageUpdateDTO> images)
        {
            var (_, toCreate, toDelete) = CategorizeItems(images);

            _repositoryWrapper.ImageRepository.DeleteRange(_mapper.Map<IEnumerable<Image>>(toDelete));
            await _repositoryWrapper.StreetcodeImageRepository.CreateRangeAsync(_mapper.Map<IEnumerable<StreetcodeImage>>(toCreate));
        }

        private async Task UpdateArtGalleryAsync(int streetcodeId, IEnumerable<ArtCreateUpdateDTO> arts, IEnumerable<StreetcodeArtSlideCreateUpdateDTO> streetcodeArtSlides)
        {
            var artsList = arts?.ToList() ?? new List<ArtCreateUpdateDTO>();
            var artSlidesList = streetcodeArtSlides?.ToList() ?? new List<StreetcodeArtSlideCreateUpdateDTO>();

            if (artsList.Count == 0 && artSlidesList.Count == 0)
            {
                return;
            }

            // Update Arts
            await UpdateArtsAsync(artsList);

            // Update StreetcodeArtSlides
            await UpdateStreetcodeArtSlidesAsync(streetcodeId, artSlidesList);
            await _repositoryWrapper.SaveChangesAsync();

            // Update StreetcodeArts (relationships between slides and arts)
            await UpdateStreetcodeArtsAsync(streetcodeId, artSlidesList);
            await _repositoryWrapper.SaveChangesAsync();
        }

        private async Task UpdateArtsAsync(IEnumerable<ArtCreateUpdateDTO> arts)
        {
            if (arts is null || !arts.Any())
            {
                return;
            }

            var (toUpdate, toCreate, toDelete) = CategorizeItems(arts);

            // Validate that images exist for new and updated arts
            var artsNeedingImageValidation = toCreate.Concat(toUpdate);
            var imageIds = artsNeedingImageValidation.Select(a => a.ImageId).Distinct().ToList();

            if (imageIds.Any())
            {
                var existingImages = await _repositoryWrapper.ImageRepository.GetAllAsync(i => imageIds.Contains(i.Id));
                var existingImageIds = new HashSet<int>(existingImages.Select(i => i.Id));

                foreach (var artDto in artsNeedingImageValidation)
                {
                    if (!existingImageIds.Contains(artDto.ImageId))
                    {
                        throw new InvalidOperationException($"Image with ID {artDto.ImageId} does not exist");
                    }
                }
            }

            // Delete arts
            if (toDelete.Any())
            {
                _repositoryWrapper.ArtRepository.DeleteRange(_mapper.Map<IEnumerable<Art>>(toDelete));
            }

            // Create new arts
            if (toCreate.Any())
            {
                var newArts = _mapper.Map<IEnumerable<Art>>(toCreate);
                await _repositoryWrapper.ArtRepository.CreateRangeAsync(newArts);
            }

            // Update existing arts
            if (toUpdate.Any())
            {
                var updatedArts = _mapper.Map<IEnumerable<Art>>(toUpdate);
                _repositoryWrapper.ArtRepository.UpdateRange(updatedArts);
            }
        }

        private async Task UpdateStreetcodeArtSlidesAsync(int streetcodeId, IEnumerable<StreetcodeArtSlideCreateUpdateDTO> streetcodeArtSlides)
        {
            if (streetcodeArtSlides is null || !streetcodeArtSlides.Any())
            {
                return;
            }

            var (toUpdate, toCreate, toDelete) = CategorizeItems(streetcodeArtSlides);

            // Delete slides
            if (toDelete.Any())
            {
                _repositoryWrapper.StreetcodeArtSlideRepository.DeleteRange(_mapper.Map<IEnumerable<StreetcodeArtSlide>>(toDelete));
            }

            // Create new slides
            if (toCreate.Any())
            {
                var newSlides = _mapper.Map<IEnumerable<StreetcodeArtSlide>>(toCreate).ToList();
                newSlides.ForEach(slide => slide.StreetcodeId = streetcodeId);

                await _repositoryWrapper.StreetcodeArtSlideRepository.CreateRangeAsync(newSlides);
            }

            // Update existing slides
            if (toUpdate.Any())
            {
                var updatedSlides = _mapper.Map<IEnumerable<StreetcodeArtSlide>>(toUpdate).ToList();
                updatedSlides.ForEach(slide => slide.StreetcodeId = streetcodeId);

                _repositoryWrapper.StreetcodeArtSlideRepository.UpdateRange(updatedSlides);
            }
        }

        private async Task UpdateStreetcodeArtsAsync(int streetcodeId, IEnumerable<StreetcodeArtSlideCreateUpdateDTO> streetcodeArtSlides)
        {
            if (streetcodeArtSlides is null || !streetcodeArtSlides.Any())
            {
                return;
            }

            // Get all existing StreetcodeArts for this streetcode to manage them properly
            var existingStreetcodeArts = await _repositoryWrapper.StreetcodeArtRepository
                .GetAllAsync(sa => sa.StreetcodeId == streetcodeId);

            // Delete all existing StreetcodeArts for this streetcode to rebuild them
            if (existingStreetcodeArts.Any())
            {
                _repositoryWrapper.StreetcodeArtRepository.DeleteRange(existingStreetcodeArts);
            }

            // Create new StreetcodeArts based on the slides
            var newStreetcodeArts = new List<StreetcodeArt>();

            foreach (var slideDto in streetcodeArtSlides)
            {
                // Skip slides marked for deletion
                if (slideDto.ModelState == ModelState.Deleted)
                {
                    continue;
                }

                // For existing slides, get the slide ID, for new slides we need to get them after creation
                int slideId = slideDto.Id;

                // If this is a new slide (Created), we need to find its ID after creation
                if (slideDto.ModelState == ModelState.Created)
                {
                    // Find the created slide by matching properties
                    var createdSlide = await _repositoryWrapper.StreetcodeArtSlideRepository
                        .GetFirstOrDefaultAsync(s => s.StreetcodeId == streetcodeId &&
                                                    s.Index == slideDto.Index &&
                                                    s.Template == slideDto.Template);
                    if (createdSlide != null)
                    {
                        slideId = createdSlide.Id;
                    }
                }

                foreach (var streetcodeArtDto in slideDto.StreetcodeArts)
                {
                    // Validate that the Art exists
                    var artExists = await _repositoryWrapper.ArtRepository
                        .GetFirstOrDefaultAsync(a => a.Id == streetcodeArtDto.ArtId);

                    if (artExists == null)
                    {
                        throw new InvalidOperationException($"Art with ID {streetcodeArtDto.ArtId} does not exist");
                    }

                    var streetcodeArt = new StreetcodeArt
                    {
                        StreetcodeId = streetcodeId,
                        StreetcodeArtSlideId = slideId,
                        ArtId = streetcodeArtDto.ArtId,
                        Index = streetcodeArtDto.Index
                    };

                    newStreetcodeArts.Add(streetcodeArt);
                }
            }

            if (newStreetcodeArts.Any())
            {
                await _repositoryWrapper.StreetcodeArtRepository.CreateRangeAsync(newStreetcodeArts);
            }
        }

        private static (IEnumerable<T> toUpdate, IEnumerable<T> toCreate, IEnumerable<T> toDelete) CategorizeItems<T>(IEnumerable<T> items)
        where T : IModelState
        {
            var toUpdate = new List<T>();
            var toCreate = new List<T>();
            var toDelete = new List<T>();

            foreach (var item in items)
            {
                switch (item.ModelState)
                {
                    case ModelState.Updated:
                        toUpdate.Add(item);
                        break;
                    case ModelState.Created:
                        toCreate.Add(item);
                        break;
                    default:
                        toDelete.Add(item);
                        break;
                }
            }

            return (toUpdate, toCreate, toDelete);
        }
    }
}