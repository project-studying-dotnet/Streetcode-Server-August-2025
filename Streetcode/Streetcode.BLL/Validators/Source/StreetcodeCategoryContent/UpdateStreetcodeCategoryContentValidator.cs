using FluentValidation;
using Streetcode.BLL.MediatR.Sources.StreetcodeCategoryContent.Update;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;

namespace Streetcode.BLL.Validators.Source.StreetcodeCategoryContent
{
    public class UpdateStreetcodeCategoryContentValidator : AbstractValidator<UpdateStreetcodeCategoryContentCommand>
    {
        public const int MaxTextLength = 4000;

        public UpdateStreetcodeCategoryContentValidator()
        {
            RuleFor(dto => dto.CategoryContentUpdateDTO.Text)
                .MaximumLength(MaxTextLength)
                .WithMessage(Errors_Validation.MaxLength.FormatWith("Text", MaxTextLength));

            RuleFor(dto => dto.CategoryContentUpdateDTO.Id)
                .NotEmpty().WithMessage(Errors_Validation.CannotBeEmpty.FormatWith("Id"));
        }
    }
}
