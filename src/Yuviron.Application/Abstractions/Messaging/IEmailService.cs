using System.Threading;
using System.Threading.Tasks;

namespace Yuviron.Application.Abstractions.Messaging;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
}