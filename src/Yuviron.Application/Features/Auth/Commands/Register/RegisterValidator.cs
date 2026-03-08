using FluentValidation;
using System;

namespace Yuviron.Application.Features.Auth.Commands.Register;

public sealed class RegisterValidator : AbstractValidator<RegisterCommand>
{
    private readonly TimeProvider _timeProvider;

    public RegisterValidator(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(320).WithMessage("Email cannot exceed 320 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one number.")
            .MaximumLength(100);

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(50).WithMessage("First name cannot exceed 50 characters.");

        RuleFor(x => x.DateOfBirth)
            .Must(BeAtLeast16YearsOld)
            .WithMessage("You must be at least 16 years old to register.");

        RuleFor(x => x.Gender)
            .IsInEnum().WithMessage("Please select a valid gender.");

        RuleFor(x => x.AcceptTerms)
            .Equal(true).WithMessage("You must agree to the Privacy Policy.");
        
        RuleFor(x => x.ArtistName)
            .NotEmpty().WithMessage("Artist name is required when registering as an artist.")
            .MaximumLength(200).WithMessage("Artist name cannot exceed 200 characters.")
            .When(x => x.IsArtist == true); 
    }

    private bool BeAtLeast16YearsOld(DateTime dateOfBirth)
    {
        var today = _timeProvider.GetUtcNow().DateTime.Date;
        return dateOfBirth.Date <= today.AddYears(-16);
    }
}