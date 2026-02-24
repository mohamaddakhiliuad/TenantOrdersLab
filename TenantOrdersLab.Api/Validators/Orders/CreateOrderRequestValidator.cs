using FluentValidation;
using TenantOrdersLab.Api.Contracts.Orders;

namespace TenantOrdersLab.Api.Validators.Orders;

public sealed class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.CustomerId)
            .GreaterThan(0)
            .WithMessage("validation: CustomerId must be a positive integer.");

        RuleFor(x => x.TotalAmount)
            .GreaterThan(0)
            .WithMessage("validation: TotalAmount must be greater than 0.");

        RuleFor(x => x.Currency)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("validation: Currency is required.")
            .Must(c => c.Trim().Length == 3)
            .WithMessage("validation: Currency must be exactly 3 letters (e.g., CAD, USD).")
            .Must(c => c.Trim().All(char.IsLetter))
            .WithMessage("validation: Currency must contain letters only.");
    }
}