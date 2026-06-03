namespace Yuviron.Application.Abstractions.Identity;

public record ClientContext(string Device, string Browser, string IpAddress);

public interface IClientContextService
{
    ClientContext GetClientContext();
}