using FluentValidation;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Users.Commands.BlockUser;

public sealed class BlockUserValidator : AbstractValidator<BlockUserCommand>
{
    public BlockUserValidator(TimeProvider timeProvider)
    {
        RuleFor(x => x.UserId).NotEqual(Guid.Empty);
        
        RuleFor(x => x.BlockType)
            .IsInEnum()
            .NotEqual(BlockType.Unknown);

        RuleFor(x => x.ReasonCode)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Description)
            .MaximumLength(500);

        RuleFor(x => x.EndsAt)
            .Must((cmd, endsAt) => endsAt > timeProvider.GetUtcNow().UtcDateTime)
            .When(x => x.EndsAt.HasValue)
            .WithMessage("Block end date must be in the future.");
    }
}