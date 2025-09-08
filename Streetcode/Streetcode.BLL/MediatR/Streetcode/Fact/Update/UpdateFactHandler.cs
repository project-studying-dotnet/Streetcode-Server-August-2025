using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode.TextContent.Fact;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Media.Image.Create;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.DAL.Entities.Media.Images;
using Streetcode.DAL.Entities.Streetcode.TextContent;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Streetcode.Fact.Update
{
    public class UpdateFactHandler : IRequestHandler<UpdateFactCommand, Result<FactDTO>>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        private readonly ILoggerService _logger;
        private readonly IMediator _mediator;

        public UpdateFactHandler(
            IRepositoryWrapper repositoryWrapper,
            IMapper mapper,
            ILoggerService logger,
            IMediator mediator)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _logger = logger;
            _mediator = mediator;
        }

        public async Task<Result<FactDTO>> Handle(UpdateFactCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var existingFact = await _repositoryWrapper.FactRepository
                    .GetSingleOrDefaultAsync(f => f.Id == request.Fact.Id);

                if (existingFact == null)
                {
                    string errorMsg = Errors_Common.NotFoundById.FormatWith("fact", request.Fact.Id);
                    _logger.LogError(request, errorMsg);
                    return Result.Fail(errorMsg);
                }

                if (request.Fact.NewImage is not null)
                {
                    var createImageResult = await _mediator.Send(
                        new CreateImageCommand(request.Fact.NewImage), cancellationToken);

                    if (createImageResult.IsFailed)
                    {
                        return Result.Fail(createImageResult.Errors[0].Message);
                    }

                    _mapper.Map(request.Fact, existingFact);
                    existingFact.ImageId = createImageResult.Value.Id;
                }
                else
                {
                    _mapper.Map(request.Fact, existingFact);
                }

                await UpdateImageDetailsAsync(request, existingFact);

                return await UpdateFactAsync(existingFact);
            }
            catch (Exception ex)
            {
                _logger.LogError(request, ex.Message);
                return Result.Fail(ex.Message);
            }
        }

        private async Task UpdateImageDetailsAsync(UpdateFactCommand request, Facts existingFact)
        {
            if (!existingFact.ImageId.HasValue)
            {
                return;
            }

            var alt = request.Fact.ImageDescription;
            if (alt is null)
            {
                return;
            }

            var imageDetails = await _repositoryWrapper.ImageDetailsRepository
                .GetSingleOrDefaultAsync(id => id.ImageId == existingFact.ImageId.Value);

            if (imageDetails != null)
            {
                if (imageDetails.Alt != alt)
                {
                    imageDetails.Alt = alt;
                    _repositoryWrapper.ImageDetailsRepository.Update(imageDetails);
                }
            }
            else
            {
                _repositoryWrapper.ImageDetailsRepository.Create(new ImageDetails
                {
                    ImageId = existingFact.ImageId.Value,
                    Alt = alt
                });
            }
        }

        private async Task<Result<FactDTO>> UpdateFactAsync(Facts existingFact)
        {
            _repositoryWrapper.FactRepository.Update(existingFact);
            var success = await _repositoryWrapper.SaveChangesAsync() > 0;

            if (!success)
            {
                string errorMsg = Errors_Common.FailedToUpdate.FormatWith("fact");
                _logger.LogError(existingFact, errorMsg);
                return Result.Fail(errorMsg);
            }

            return Result.Ok(_mapper.Map<FactDTO>(existingFact));
        }
    }
}
