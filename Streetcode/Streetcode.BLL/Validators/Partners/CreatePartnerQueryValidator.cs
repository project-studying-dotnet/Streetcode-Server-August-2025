using FluentValidation;
using Streetcode.BLL.MediatR.Partners.Create;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;

namespace Streetcode.BLL.Validators.Partners;

public class CreatePartnerQueryValidator : AbstractValidator<CreatePartnerQuery>
{
    public CreatePartnerQueryValidator()
    {
        RuleFor(x => x.newPartner)
            .NotNull()
            .WithMessage(Errors_Validation.IsRequiredData.FormatWith("Partner"));

        When(x => x.newPartner != null, () =>
        {
            RuleFor(x => x.newPartner)
                .SetValidator(new CreatePartnerDTOValidator());
        });
    }
}