using FluentValidation;
using Streetcode.BLL.DTO.AdditionalContent.Tag;

namespace Streetcode.BLL.Validators.AdditionalContent.Tag;

public class TagValidator : AbstractValidator<CreateTagDTO>
{
    public const int TitleMaxLength = 50;
    public TagValidator()
    {
        RuleFor(dto => dto.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(TitleMaxLength).WithMessage($"Title cannot exceed {TitleMaxLength} characters.");
    }
}
