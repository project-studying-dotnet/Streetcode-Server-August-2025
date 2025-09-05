using FluentValidation;
using Streetcode.BLL.MediatR.Streetcode.Streetcode.Create;
using Streetcode.BLL.Validators.AdditionalContent.Tag;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.Validators.Streetcode;

public class CreateStreetcodeValidator : AbstractValidator<StreetcodeCreateCommand>
{
    private readonly IRepositoryWrapper _repositoryWrapper;

    public CreateStreetcodeValidator(
        IRepositoryWrapper repositoryWrapper,
        BaseStreetcodeValidator baseStreetcodeValidator,
        TagValidator tagValidator)
    {
        _repositoryWrapper = repositoryWrapper;

        RuleFor(c => c.NewStreetcode).SetValidator(baseStreetcodeValidator);

        RuleFor(c => c.NewStreetcode.Index)
            .MustAsync(BeUniqueIndex).WithMessage("Index must be unique.");

        RuleFor(c => c.NewStreetcode.ImagesDetails)
            .NotEmpty()
            .WithMessage("At least one image detail is required.");

        RuleForEach(c => c.NewStreetcode.ImagesDetails.Select(x => x.ImageId))
            .MustAsync(HasExistingImage)
            .WithMessage("One or more images do not exist.")
            .OverridePropertyName("Streetcode.ImagesDetails.ImageId");

        RuleForEach(c => c.NewStreetcode.Tags).SetValidator(tagValidator);
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
