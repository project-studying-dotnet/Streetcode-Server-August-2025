using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Streetcode.BLL.DTO.Sources;

namespace Streetcode.BLL.Validators.Source.SourceLinkCategory
{
    public class CreateSourceLinkCategoryValidator : AbstractValidator<SourceLinkCategoryCreateDTO>
    {
        public CreateSourceLinkCategoryValidator()
        {
            RuleFor(dto => dto.Title)
            .MaximumLength(23).WithMessage("Category can`t be more than 23 symbols.");
        }
    }
}
