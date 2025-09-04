using FluentValidation;
using Streetcode.BLL.MediatR.Sources.SourceLinkCategory.Create;

namespace Streetcode.BLL.Validators.Source.SourceLinkCategory
{
    public class CreateSourceLinkCategoryValidator : AbstractValidator<CreateSourceLinkCategoryCommand>
    {
        public CreateSourceLinkCategoryValidator()
        {
            RuleFor(c => c.SourceLinkCategoryCreateDTO.Title)
            .MaximumLength(23).WithMessage("Category can`t be more than 23 symbols.");
        }
    }
}
