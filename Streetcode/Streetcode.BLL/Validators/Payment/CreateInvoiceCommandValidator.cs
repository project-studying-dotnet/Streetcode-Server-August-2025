using FluentValidation;
using Streetcode.BLL.MediatR.Payment;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.BLL.Validators.Helpers;

namespace Streetcode.BLL.Validators.Payment;

public class CreateInvoiceCommandValidator : AbstractValidator<CreateInvoiceCommand>
{
    public CreateInvoiceCommandValidator()
    {
        RuleFor(x => x.Payment)
            .NotNull()
            .WithMessage(Errors_Validation.IsRequiredData.FormatWith("Payment"))
            .DependentRules(() =>
            {
                RuleFor(x => x.Payment!.Amount)
                    .GreaterThan(0)
                    .WithMessage(Errors_Validation.GreaterThan.FormatWith("Amount", 0));

                RuleFor(x => x.Payment!.RedirectUrl)
                    .Must(ValidationHelper.BeValidUrl)
                    .When(x => !string.IsNullOrWhiteSpace(x.Payment!.RedirectUrl))
                    .WithMessage(Errors_Validation.ValidUrl.FormatWith("RedirectUrl"));
            });
    }
}