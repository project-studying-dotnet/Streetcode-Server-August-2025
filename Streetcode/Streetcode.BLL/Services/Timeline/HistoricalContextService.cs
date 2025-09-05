using FluentResults;
using Org.BouncyCastle.Utilities;
using Streetcode.BLL.DTO.Timeline.HistoricalContext;
using Streetcode.BLL.Interfaces.Timeline;
using Streetcode.DAL.Entities.Timeline;
using Streetcode.DAL.Repositories.Interfaces.Base;
using HistoricalContextEntity = Streetcode.DAL.Entities.Timeline.HistoricalContext;

namespace Streetcode.BLL.Services.Timeline
{
    public class HistoricalContextService : IHistoricalContextService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;

        public HistoricalContextService(IRepositoryWrapper repositoryWrapper)
        {
            _repositoryWrapper = repositoryWrapper;
        }

        public async Task<Result> CheckForDuplicateTitlesAsync(IEnumerable<HistoricalContextRequestDto> contexts)
        {
            if (contexts is null)
            {
                return Result.Fail("Input contexts cannot be null.");
            }

            var newTitles = contexts
                .Where(x => !x.Id.HasValue && !string.IsNullOrWhiteSpace(x.Title))
                .Select(x => x.Title!)
                .ToList();

            if (!newTitles.Any())
            {
                return Result.Ok();
            }

            var existingContexts = await _repositoryWrapper.HistoricalContextRepository
                .GetAllAsync(hc => newTitles.Contains(hc.Title!));

            if (existingContexts.Any())
            {
                var duplicateTitle = existingContexts.First().Title;
                return Result.Fail($"A historical context with the title '{duplicateTitle}' already exists.");
            }

            return Result.Ok();
        }

        public async Task<Result> BuildHistoricalContextLinksAsync(
            TimelineItem timelineItem,
            IEnumerable<HistoricalContextRequestDto> contexts)
        {
            if (contexts is null)
            {
                return Result.Fail("Input contexts cannot be null.");
            }

            if (timelineItem is null)
            {
                return Result.Fail("TimelineItem cannot be null.");
            }

            if (timelineItem.HistoricalContextTimelines.Any())
            {
                _repositoryWrapper.HistoricalContextTimelineRepository.DeleteRange(timelineItem.HistoricalContextTimelines);
                timelineItem.HistoricalContextTimelines.Clear();
            }

            var seenIds = new HashSet<int>();
            var seenNewTitles = new HashSet<string>();

            foreach (var contextDto in contexts)
            {
                HistoricalContextEntity? historicalContext = null;

                if (contextDto.Id.HasValue)
                {
                    if (!seenIds.Add(contextDto.Id.Value))
                    {
                        continue;
                    }

                    historicalContext = await _repositoryWrapper.HistoricalContextRepository
                        .GetFirstOrDefaultAsync(hc => hc.Id == contextDto.Id.Value);

                    if (historicalContext == null)
                    {
                        string errorMsg = $"Historical context with Id={contextDto.Id.Value} not found";
                        return Result.Fail(errorMsg);
                    }
                }
                else
                {
                    if (!seenNewTitles.Add(contextDto.Title!))
                    {
                        continue;
                    }

                    historicalContext = new HistoricalContextEntity { Title = contextDto.Title! };
                    await _repositoryWrapper.HistoricalContextRepository.CreateAsync(historicalContext);
                }

                timelineItem.HistoricalContextTimelines.Add(new HistoricalContextTimeline
                {
                    HistoricalContext = historicalContext,
                    Timeline = timelineItem
                });
            }

            return Result.Ok();
        }

        public Result RemoveObsoleteLinks(TimelineItem timelineItem, IEnumerable<HistoricalContextRequestDto> newContexts)
        {
            if (newContexts is null)
            {
                return Result.Fail("Input contexts cannot be null.");
            }

            if (timelineItem is null)
            {
                return Result.Fail("TimelineItem cannot be null.");
            }

            var incomingContextIds = newContexts
                .Where(x => x.Id.HasValue)
                .Select(x => x.Id!.Value)
                .ToList();

            var toRemove = timelineItem.HistoricalContextTimelines
                .Where(hct => !incomingContextIds.Contains(hct.HistoricalContextId))
                .ToList();

            if (toRemove.Any())
            {
                _repositoryWrapper.HistoricalContextTimelineRepository.DeleteRange(toRemove);
            }

            return Result.Ok();
        }
    }
}