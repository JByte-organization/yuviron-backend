namespace Yuviron.Application.Features.Client.Settings.Queries.GetMyDevices;

public record UserDeviceDto(Guid Id, string DeviceName, string BrowserName, string LastIpAddress, DateTime LastUsedAt, DateTime CreatedAt);