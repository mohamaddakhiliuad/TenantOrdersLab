using FluentValidation;
using TenantOrdersLab.Api.Contracts.Orders;

namespace TenantOrdersLab.Api.Validators.Orders;

public sealed class CancelOrderRequestValidator : AbstractValidator<CancelOrderRequest>
{
    public CancelOrderRequestValidator()
    {
        RuleFor(x => x.OrderId)
            .GreaterThan(0)
            .WithMessage("validation: OrderId must be a positive integer.");

        RuleFor(x => x.Reason)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("validation: Reason is required.")
            .MinimumLength(3).WithMessage("validation: Reason must be at least 3 characters.")
            .MaximumLength(200).WithMessage("validation: Reason must be at most 200 characters.");
        RuleFor(x => x.ExpectedRowVersion)
           .NotNull().WithMessage("validation: ExpectedRowVersion is required.")
           .Must(rv => rv.Length > 0).WithMessage("validation: ExpectedRowVersion must not be empty.");
    }
}