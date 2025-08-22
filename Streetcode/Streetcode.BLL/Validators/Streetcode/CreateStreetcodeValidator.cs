using FluentValidation;
using Streetcode.BLL.DTO.Streetcode.Create;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.Validators.Streetcode;

public class CreateStreetcodeValidator : AbstractValidator<StreetcodeCreateDTO>
{
    private readonly IRepositoryWrapper _repositoryWrapper;

    public CreateStreetcodeValidator(IRepositoryWrapper repositoryWrapper, BaseStreetcodeValidator baseStreetcodeValidator)
    {
        _repositoryWrapper = repositoryWrapper;

        RuleFor(dto => dto).SetValidator(baseStreetcodeValidator);

        RuleFor(dto => dto.Index)
            .MustAsync(BeUniqueIndex).WithMessage("Index must be unique.");

        RuleFor(dto => dto.ImagesDetails)
            .NotEmpty()
            .WithMessage("At least one image detail is required.");

        RuleForEach(dto => dto.ImagesDetails.Select(x => x.ImageId))
            .MustAsync(HasExistingImage)
            .WithMessage("One or more images do not exist.");
    }

    private async Task<bool> BeUniqueIndex(int index, CancellationToken cancellationToken)
    {
        var existingStreetcode = await _repositoryWrapper.StreetcodeRepository.GetFirstOrDefaultAsync(sc => sc.Index == index);

        return existingStreetcode == null;
    }

    private async Task<bool> HasExistingImage(int imageId, CancellationToken token)
    {
        var image = await _repositoryWrapper.ImageRepository.GetFirstOrDefaultAsync(i => i.Id == imageId);

        return image != null;
    }
}
