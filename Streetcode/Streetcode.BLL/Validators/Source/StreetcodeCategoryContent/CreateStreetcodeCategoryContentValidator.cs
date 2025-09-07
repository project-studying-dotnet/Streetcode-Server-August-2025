using FluentValidation;
using Streetcode.BLL.MediatR.Sources.StreetcodeCategoryContent.Create;

namespace Streetcode.BLL.Validators.Source.StreetcodeCategoryContent
{
    public class CreateStreetcodeCategoryContentValidator : AbstractValidator<CreateStreetcodeCategoryContentCommand>
    {
        public const int MaxTextLength = 4000;

        public CreateStreetcodeCategoryContentValidator()
        {
            RuleFor(dto => dto.CreateCategoryContentDto.Text)
            .MaximumLength(MaxTextLength).WithMessage("Text can`t be more than 4000 symbols.");
        }
    }
}
