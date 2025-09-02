using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Streetcode.BLL.DTO.Sources;

namespace Streetcode.BLL.Validators.Source.StreetcodeCategoryContent
{
    public class UpdateStreetcodeCategoryContentValidator : AbstractValidator<CategoryContentUpdateDTO>
    {
        public UpdateStreetcodeCategoryContentValidator()
        {
            RuleFor(dto => dto.Text)
            .MaximumLength(4000).WithMessage("Text can`t be more than 4000 symbols.");

            RuleFor(dto => dto.Id)
                .NotEmpty().WithMessage("Id can`t be null.");
        }
    }
}
