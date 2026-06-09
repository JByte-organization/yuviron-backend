using FluentValidation;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Client.Complaints.Commands.CreateComplaint;

public sealed class CreateComplaintCommandValidator : AbstractValidator<CreateComplaintCommand>
{
    public CreateComplaintCommandValidator()
    {
        RuleFor(x => x.TargetId).NotEmpty();

        RuleFor(x => x.ReasonCode)
            .IsInEnum()
            .WithMessage("Invalid complaint reason.");

        RuleFor(x => x.Comment)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrWhiteSpace(x.Comment));
    }
}
