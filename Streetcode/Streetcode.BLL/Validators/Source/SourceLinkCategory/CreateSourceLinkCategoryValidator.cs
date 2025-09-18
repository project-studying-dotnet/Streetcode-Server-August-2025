using FluentValidation;
using Streetcode.BLL.MediatR.Sources.SourceLinkCategory.Create;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;

namespace Streetcode.BLL.Validators.Source.SourceLinkCategory
{
    public class CreateSourceLinkCategoryValidator : AbstractValidator<CreateSourceLinkCategoryCommand>
    {
        public const int MaxTitleLength = 23;

        public CreateSourceLinkCategoryValidator()
        {
            RuleFor(c => c.SourceLinkCategoryCreateDTO.Title)
                .MaximumLength(MaxTitleLength)
                    .WithMessage(Errors_Validation.MaxLength.FormatWith("Title", MaxTitleLength));
        }
    }
}
