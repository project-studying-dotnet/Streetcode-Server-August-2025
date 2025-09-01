using FluentValidation;
using Streetcode.BLL.DTO.Toponyms;

namespace Streetcode.BLL.Validators.Streetcode.Toponyms;

public class StreetcodeToponymValidator : AbstractValidator<StreetcodeToponymCreateUpdateDTO>
{
    public const int StreetNameMaxLength = 150;

    public StreetcodeToponymValidator()
    {
        RuleFor(dto => dto.StreetName)
            .NotEmpty().WithMessage($"CannotBeEmpty")
            .MaximumLength(StreetNameMaxLength).WithMessage("MaxLength");

        RuleFor(dto => dto.ModelState)
            .IsInEnum().WithMessage("Invalid");
    }
}