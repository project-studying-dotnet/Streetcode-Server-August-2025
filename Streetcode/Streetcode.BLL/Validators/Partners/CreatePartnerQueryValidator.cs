using FluentValidation;
using Streetcode.BLL.MediatR.Partners.Create;

namespace Streetcode.BLL.Validators.Partners;

public class CreatePartnerQueryValidator : AbstractValidator<CreatePartnerQuery>
{
    public CreatePartnerQueryValidator()
    {
        RuleFor(x => x.newPartner)
            .NotNull()
            .WithMessage("Partner data is required.");

        When(x => x.newPartner != null, () =>
        {
            RuleFor(x => x.newPartner)
                .SetValidator(new CreatePartnerDTOValidator());
        });
    }
}