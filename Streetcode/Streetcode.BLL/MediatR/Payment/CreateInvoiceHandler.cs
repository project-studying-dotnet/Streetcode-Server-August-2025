using FluentResults;
using MediatR;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Interfaces.Payment;
using Streetcode.DAL.Entities.Payment;

namespace Streetcode.BLL.MediatR.Payment;

public class CreateInvoiceHandler : IRequestHandler<CreateInvoiceCommand, Result<InvoiceInfo>>
{
    private const int _hryvnyaCurrencyCode = 980;
    private const int _currencyMultiplier = 100;
    private readonly IPaymentService _paymentService;
    private readonly ILoggerService _logger;

    public CreateInvoiceHandler(IPaymentService paymentService, ILoggerService logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    public async Task<Result<InvoiceInfo>> Handle(CreateInvoiceCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Creating invoice for amount: {request.Payment.Amount} UAH");

        var invoice = new Invoice(request.Payment.Amount * _currencyMultiplier, _hryvnyaCurrencyCode, new MerchantPaymentInfo { Destination = "Добровільний внесок на статутну діяльність ГО «Історична Платформа»" }, request.Payment.RedirectUrl);
        var result = await _paymentService.CreateInvoiceAsync(invoice);

        _logger.LogInformation($"Invoice created successfully with ID: {result.InvoiceId}");

        return Result.Ok(result);
    }
}
