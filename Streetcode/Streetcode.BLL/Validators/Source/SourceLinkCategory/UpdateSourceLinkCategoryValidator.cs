using FluentValidation;
using Streetcode.BLL.MediatR.Sources.SourceLinkCategory.Update;

namespace Streetcode.BLL.Validators.Source.SourceLinkCategory
{
    public class UpdateSourceLinkCategoryValidator : AbstractValidator<UpdateSourceLinkCategoryCommand>
    {
        public UpdateSourceLinkCategoryValidator()
        {
            RuleFor(c => c.SourceLinkCategoryUpdate.Id)
                .NotEmpty().WithMessage("Id can`t be null.");

            RuleFor(c => c.SourceLinkCategoryUpdate.Title)
            .MaximumLength(23).WithMessage("Category can`t be more than 23 symbols.");
        }
    }
}
