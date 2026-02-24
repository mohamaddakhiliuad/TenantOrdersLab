using FluentValidation;
using TenantOrdersLab.Api.Contracts.Orders;

namespace TenantOrdersLab.Api.Validators.Orders
{
    public class PlaceOrderRequestValidatior : AbstractValidator<PlaceOrderRequest>
    {
        public PlaceOrderRequestValidatior()
        {
            RuleFor(x => x.OrderID)
                .GreaterThan(0)
                .WithMessage("validation: OrderId must be a positive integer.");
            RuleFor(x => x.ExpectedRowVersion)
                .NotNull().WithMessage("validation: ExpectedRowVersion is required.")
                .Must(rv => rv.Length > 0).WithMessage("validation: ExpectedRowVersion must not be empty.");
        }
    }
}
