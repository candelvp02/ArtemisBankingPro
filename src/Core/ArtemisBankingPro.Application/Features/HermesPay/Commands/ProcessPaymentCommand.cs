using ArtemisBankingPro.Application.DTOs.HermesPay;
using ArtemisBankingPro.Application.Services.Interfaces;
using FluentValidation;
using MediatR;

namespace ArtemisBankingPro.Application.Features.HermesPay.Commands;

public record ProcessPaymentCommand(
    int CommerceId, string CardNumber, DateTime ExpirationDate, string Cvc, decimal Amount)
    : IRequest<PaymentResultDto>;

public class ProcessPaymentCommandValidator : AbstractValidator<ProcessPaymentCommand>
{
    public ProcessPaymentCommandValidator()
    {
        RuleFor(x => x.CommerceId).GreaterThan(0);
        RuleFor(x => x.CardNumber).NotEmpty().Length(16);
        RuleFor(x => x.Cvc).NotEmpty().Length(3);
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}

public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, PaymentResultDto>
{
    private readonly IHermesPayService _hermesPayService;

    public ProcessPaymentCommandHandler(IHermesPayService hermesPayService)
    {
        _hermesPayService = hermesPayService;
    }

    public Task<PaymentResultDto> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken) =>
        _hermesPayService.ProcessPaymentAsync(new ProcessPaymentDto
        {
            CommerceId = request.CommerceId,
            CardNumber = request.CardNumber,
            ExpirationDate = request.ExpirationDate,
            Cvc = request.Cvc,
            Amount = request.Amount
        });
}