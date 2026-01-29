using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MimeKit.Text;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions.Messaging;

namespace Yuviron.Infrastructure.Services;

internal class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public SmtpEmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        var email = new MimeMessage();

        email.From.Add(new MailboxAddress("Yuviron Team", _configuration["Email:Username"]));

        email.To.Add(MailboxAddress.Parse(to));

        email.Subject = subject;
        email.Body = new TextPart(TextFormat.Html) { Text = body };

        using var smtp = new SmtpClient();

        await smtp.ConnectAsync(
            _configuration["Email:Host"],
            int.Parse(_configuration["Email:Port"]!),
            SecureSocketOptions.StartTls,
            cancellationToken
        );

        await smtp.AuthenticateAsync(
            _configuration["Email:Username"],
            _configuration["Email:Password"],
            cancellationToken
        );

        await smtp.SendAsync(email, cancellationToken);
        await smtp.DisconnectAsync(true, cancellationToken);
    }
}