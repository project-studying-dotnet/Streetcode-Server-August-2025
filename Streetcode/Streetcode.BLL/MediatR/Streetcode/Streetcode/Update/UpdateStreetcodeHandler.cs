using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.AdditionalContent.Tag;
using Streetcode.BLL.DTO.Interfaces;
using Streetcode.BLL.DTO.Media.Images;
using Streetcode.BLL.Enums;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Interfaces.Redis;
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
        private readonly IRedisService<StreetcodeContent> _redisService;

        public UpdateStreetcodeHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, ILoggerService logger, IRedisService<StreetcodeContent> redisService)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _logger = logger;
            _redisService = redisService;
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

                    var redisKeys = new[]
                        {
                            $"streetcodeById:{existingEntity.Id}",
                            $"streetcodeByIndex:{existingEntity.Index}",
                            $"streetcodeByUrl:{existingEntity.TransliterationUrl}",
                            $"streetcodeShortById:{existingEntity.Id}"
                        };

                    await _redisService.DeleteAsync(redisKeys, cancellationToken);
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