using System;
using System.Linq;
using FluentValidation;
using Microsoft.Extensions.Options; 
using Yuviron.Application.Configuration; 

namespace Yuviron.Application.Features.Client.Payments.Commands.CreateCheckoutSession;

public sealed class CreateCheckoutSessionValidator : AbstractValidator<CreateCheckoutSessionCommand>
{
    private readonly string[] _allowedHosts;

    public CreateCheckoutSessionValidator(IOptions<CorsSettingsOptions> corsOptions)
    {
        var allowedOrigins = corsOptions.Value.AllowedOrigins ?? Array.Empty<string>();
        
        _allowedHosts = allowedOrigins
            .Select(origin => Uri.TryCreate(origin, UriKind.Absolute, out var uri) ? uri.Host.ToLower() : string.Empty)
            .Where(host => !string.IsNullOrEmpty(host))
            .ToArray();

        RuleFor(x => x.PlanId).NotEmpty();

        RuleFor(x => x.SuccessUrl)
            .NotEmpty()
            .Must(BeAValidAllowedUrl).WithMessage("Success URL contains an untrusted domain. Open Redirects are forbidden.");

        RuleFor(x => x.CancelUrl)
            .NotEmpty()
            .Must(BeAValidAllowedUrl).WithMessage("Cancel URL contains an untrusted domain. Open Redirects are forbidden.");
    }

    private bool BeAValidAllowedUrl(string url)
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return _allowedHosts.Contains(uri.Host.ToLower());
        }
        return false;
    }
}