using FluentValidation;
using Streetcode.BLL.MediatR.Sources.SourceLinkCategory.Update;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;

namespace Streetcode.BLL.Validators.Source.SourceLinkCategory
{
    public class UpdateSourceLinkCategoryValidator : AbstractValidator<UpdateSourceLinkCategoryCommand>
    {
        public const int MaxTitleLength = 23;

        public UpdateSourceLinkCategoryValidator()
        {
            RuleFor(c => c.SourceLinkCategoryUpdate.Id)
                .NotEmpty()
                .WithMessage(Errors_Validation.CannotBeEmpty.FormatWith("Id"));

            RuleFor(c => c.SourceLinkCategoryUpdate.Title)
                .MaximumLength(MaxTitleLength)
                .WithMessage(Errors_Validation.MaxLength.FormatWith("Title", MaxTitleLength));
        }
    }
}
