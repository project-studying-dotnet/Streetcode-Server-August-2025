using FluentValidation;
using Streetcode.BLL.MediatR.Sources.StreetcodeCategoryContent.Create;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;

namespace Streetcode.BLL.Validators.Source.StreetcodeCategoryContent
{
    public class CreateStreetcodeCategoryContentValidator : AbstractValidator<CreateStreetcodeCategoryContentCommand>
    {
        public const int MaxTextLength = 4000;

        public CreateStreetcodeCategoryContentValidator()
        {
            RuleFor(dto => dto.CreateCategoryContentDto.Text)
                .MaximumLength(MaxTextLength)
                .WithMessage(Errors_Validation.MaxLength.FormatWith("Text", MaxTextLength));
        }
    }
}
