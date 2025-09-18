using FluentValidation.TestHelper;
using Streetcode.BLL.DTO.Payment;
using Streetcode.BLL.MediatR.Payment;
using Streetcode.BLL.Validators.Payment;
using Xunit;

namespace Streetcode.XUnitTest.BLL.Validators.Payment
{
    public class CreateInvoiceCommandValidatorTests
    {
        private readonly CreateInvoiceCommandValidator _validator;

        public CreateInvoiceCommandValidatorTests()
        {
            _validator = new CreateInvoiceCommandValidator();
        }

        [Fact]
        public void Should_Have_Error_When_Amount_Is_Zero()
        {
            var paymentDto = new PaymentDTO { Amount = 0, RedirectUrl = "https://valid.url" };
            var command = new CreateInvoiceCommand(paymentDto);
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(c => c.Payment.Amount);
        }

        [Fact]
        public void Should_Have_Error_When_RedirectUrl_Is_Invalid()
        {
            var paymentDto = new PaymentDTO { Amount = 100, RedirectUrl = "invalid-url" };
            var command = new CreateInvoiceCommand(paymentDto);
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(c => c.Payment.RedirectUrl);
        }

        [Fact]
        public void Should_Not_Have_Error_For_Valid_Command()
        {
            var paymentDto = new PaymentDTO { Amount = 100, RedirectUrl = "https://valid.url" };
            var command = new CreateInvoiceCommand(paymentDto);
            var result = _validator.TestValidate(command);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
