using FluentValidation;
using Streetcode.BLL.MediatR.Sources.StreetcodeCategoryContent.Update;

namespace Streetcode.BLL.Validators.Source.StreetcodeCategoryContent
{
    public class UpdateStreetcodeCategoryContentValidator : AbstractValidator<UpdateStreetcodeCategoryContentCommand>
    {
        public const int MaxTextLength = 4000;

        public UpdateStreetcodeCategoryContentValidator()
        {
            RuleFor(dto => dto.CategoryContentUpdateDTO.Text)
            .MaximumLength(MaxTextLength).WithMessage("Text can`t be more than 4000 symbols.");

            RuleFor(dto => dto.CategoryContentUpdateDTO.Id)
                .NotEmpty().WithMessage("Id can`t be null.");
        }
    }
}
