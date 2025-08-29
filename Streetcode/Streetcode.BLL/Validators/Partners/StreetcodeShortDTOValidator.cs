using FluentValidation;
using Streetcode.BLL.DTO.Streetcode;

namespace Streetcode.BLL.Validators.Partners;

public class StreetcodeShortDTOValidator : AbstractValidator<StreetcodeShortDTO>
{
    public StreetcodeShortDTOValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Streetcode Id must be greater than 0.");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Streetcode title is required.")
            .MaximumLength(255)
            .WithMessage("Streetcode title cannot exceed 255 characters.");
    }
}