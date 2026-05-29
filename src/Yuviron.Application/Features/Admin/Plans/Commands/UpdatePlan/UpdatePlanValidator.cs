using FluentValidation;

namespace Yuviron.Application.Features.Admin.Plans.Commands.UpdatePlan;

public sealed class UpdatePlanValidator : AbstractValidator<UpdatePlanCommand>
{
    public UpdatePlanValidator()
    {
        RuleFor(v => v.Id).NotEmpty().WithMessage("Id is required");
        
        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

        RuleFor(v => v.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price cannot be negative");

        RuleFor(v => v.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .Length(3).WithMessage("Currency must be a 3-letter code (e.g., USD)");

        RuleFor(v => v.Period)
            .IsInEnum().WithMessage("Invalid plan period");
        
        RuleFor(v => v.Type)
            .IsInEnum().WithMessage("Invalid plan type");
    }
}