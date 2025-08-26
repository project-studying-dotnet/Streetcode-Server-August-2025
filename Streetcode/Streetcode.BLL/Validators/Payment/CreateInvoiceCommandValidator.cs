using FluentValidation;
using Streetcode.BLL.MediatR.Payment;

namespace Streetcode.BLL.Validators.Payment;

public class CreateInvoiceCommandValidator : AbstractValidator<CreateInvoiceCommand>
{
    public CreateInvoiceCommandValidator()
    {
        RuleFor(x => x.Payment)
            .NotNull()
            .WithMessage("Payment information is required.");

        RuleFor(x => x.Payment.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than zero.");

        RuleFor(x => x.Payment.RedirectUrl)
            .Must(BeValidUrl)
            .When(x => !string.IsNullOrWhiteSpace(x.Payment.RedirectUrl))
            .WithMessage("RedirectUrl must be a valid absolute URL.");
    }

    private static bool BeValidUrl(string? url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}