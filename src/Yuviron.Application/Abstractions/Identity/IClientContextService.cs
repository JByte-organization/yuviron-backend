namespace Yuviron.Application.Abstractions.Identity;

public record ClientContext(string Device, string Browser, string IpAddress, string Fingerprint);

public interface IClientContextService
{
    ClientContext GetClientContext();
}