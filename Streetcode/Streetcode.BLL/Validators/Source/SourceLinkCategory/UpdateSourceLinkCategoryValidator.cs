using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Streetcode.BLL.DTO.Sources;

namespace Streetcode.BLL.Validators.Source.SourceLinkCategory
{
    public class UpdateSourceLinkCategoryValidator : AbstractValidator<SourceLinkCategoryUpdateDTO>
    {
        public UpdateSourceLinkCategoryValidator()
        {
            RuleFor(dto => dto.Id)
                .NotEmpty().WithMessage("Id can`t be null.");

            RuleFor(dto => dto.Title)
            .MaximumLength(23).WithMessage("Category can`t be more than 23 symbols.");
        }
    }
}
