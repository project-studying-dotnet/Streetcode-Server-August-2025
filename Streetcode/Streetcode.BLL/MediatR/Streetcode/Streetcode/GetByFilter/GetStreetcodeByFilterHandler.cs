using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MimeKit.Text;
using Streetcode.BLL.DTO.Streetcode;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Entities.Streetcode.TextContent;
using Streetcode.DAL.Entities.Timeline;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Streetcode.Streetcode.GetByFilter
{
    public class GetStreetcodeByFilterHandler : IRequestHandler<GetStreetcodeByFilterQuery, Result<List<StreetcodeFilterResultDTO>>>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILoggerService _logger;

        public GetStreetcodeByFilterHandler(IRepositoryWrapper repositoryWrapper, ILoggerService logger)
        {
            _repositoryWrapper = repositoryWrapper;
            _logger = logger;
        }

        public async Task<Result<List<StreetcodeFilterResultDTO>>> Handle(GetStreetcodeByFilterQuery request, CancellationToken cancellationToken)
        {
            string searchQuery = request.Filter.SearchQuery;
            _logger.LogInformation($"Starting streetcode search with query: '{searchQuery}'");

            var results = new List<StreetcodeFilterResultDTO>();

            var streetcodes = await _repositoryWrapper.StreetcodeRepository.GetAllAsync(
                 predicate: x =>
                    (x.Status == DAL.Enums.StreetcodeStatus.Published) &&
                    (x.Title.Contains(searchQuery) ||
                    (x.Alias != null && x.Alias.Contains(searchQuery)) ||
                    x.Teaser.Contains(searchQuery)));

            _logger.LogInformation($"Found {streetcodes.Count()} streetcodes matching basic criteria");

            foreach (var streetcode in streetcodes)
            {
                if (streetcode.Title.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(CreateFilterResult(streetcode, streetcode.Title));
                    continue;
                }

                if (!string.IsNullOrEmpty(streetcode.Alias) && streetcode.Alias.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(CreateFilterResult(streetcode, streetcode.Alias));
                    continue;
                }

                if (streetcode.Teaser.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(CreateFilterResult(streetcode, streetcode.Teaser));
                    continue;
                }

                if (streetcode.TransliterationUrl.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(CreateFilterResult(streetcode, streetcode.TransliterationUrl));
                }
            }

            _logger.LogInformation($"After streetcode search: {results.Count} results found");

            var texts = await _repositoryWrapper.TextRepository.GetAllAsync(
                include: i => i.Include(x => x.Streetcode),
                predicate: x => x.Streetcode.Status == DAL.Enums.StreetcodeStatus.Published);

            _logger.LogInformation($"Searching in {texts.Count()} texts");
            foreach (var text in texts)
            {
                if (text.Title.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(CreateFilterResult(text.Streetcode, text.Title, "Текст", "text"));
                    continue;
                }

                if (!string.IsNullOrEmpty(text.TextContent) && text.TextContent.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(CreateFilterResult(text.Streetcode, text.TextContent, "Текст", "text"));
                }
            }

            _logger.LogInformation($"After text search: {results.Count} results found");

            var facts = await _repositoryWrapper.FactRepository.GetAllAsync(
                include: i => i.Include(x => x.Streetcode),
                predicate: x => x.Streetcode.Status == DAL.Enums.StreetcodeStatus.Published);

            _logger.LogInformation($"Searching in {facts.Count()} facts");
            foreach (var fact in facts)
            {
                if (fact.Title.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) || fact.FactContent.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(CreateFilterResult(fact.Streetcode, fact.Title, "Wow-факти", "wow-facts"));
                }
            }

            _logger.LogInformation($"After facts search: {results.Count} results found");

            var timelineItems = await _repositoryWrapper.TimelineRepository.GetAllAsync(
                include: i => i.Include(x => x.Streetcode),
                predicate: x => x.Streetcode.Status == DAL.Enums.StreetcodeStatus.Published);

            _logger.LogInformation($"Searching in {timelineItems.Count()} timeline items");
            foreach (var timelineItem in timelineItems)
            {
                if (timelineItem.Title.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)
                    || (!string.IsNullOrEmpty(timelineItem.Description) && timelineItem.Description.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)))
                {
                    results.Add(CreateFilterResult(timelineItem.Streetcode, timelineItem.Title, "Хронологія", "timeline"));
                }
            }

            _logger.LogInformation($"After timeline search: {results.Count} results found");

            var streetcodeArts = await _repositoryWrapper.ArtRepository.GetAllAsync(
                include: i => i.Include(x => x.StreetcodeArts),
                predicate: x => x.StreetcodeArts.Any(art => art.Streetcode != null && art.Streetcode.Status == DAL.Enums.StreetcodeStatus.Published));

            _logger.LogInformation($"Searching in {streetcodeArts.Count()} art items");
            foreach (var streetcodeArt in streetcodeArts)
            {
                if (!string.IsNullOrEmpty(streetcodeArt.Description) && streetcodeArt.Description.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                {
                    streetcodeArt.StreetcodeArts.ForEach(art =>
                    {
                        if (art.Streetcode == null)
                        {
                            return;
                        }

                        results.Add(CreateFilterResult(art.Streetcode, streetcodeArt.Description, "Арт-галерея", "art-gallery"));
                    });
                    continue;
                }
            }

            _logger.LogInformation($"Search completed. Total results found: {results.Count}");
            return results;
        }

        private StreetcodeFilterResultDTO CreateFilterResult(StreetcodeContent streetcode, string content, string? sourceName = null, string? blockName = null)
        {
            return new StreetcodeFilterResultDTO
            {
                StreetcodeId = streetcode.Id,
                StreetcodeTransliterationUrl = streetcode.TransliterationUrl,
                StreetcodeIndex = streetcode.Index,
                BlockName = blockName,
                Content = content,
                SourceName = sourceName,
            };
        }
    }
}