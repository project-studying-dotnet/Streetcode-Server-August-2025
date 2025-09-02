using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Streetcode.BLL.DTO.Sources;

namespace Streetcode.BLL.Validators.Source.StreetcodeCategoryContent
{
    public class CreateStreetcodeCategoryContentValidator : AbstractValidator<CategoryContentCreateDTO>
    {
        public CreateStreetcodeCategoryContentValidator()
        {
            RuleFor(dto => dto.Text)
            .MaximumLength(4000).WithMessage("Text can`t be more than 4000 symbols.");
        }
    }
}
