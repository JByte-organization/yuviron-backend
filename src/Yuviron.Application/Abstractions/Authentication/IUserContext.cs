namespace Yuviron.Application.Abstractions.Authentication;

public interface IUserContext
{
    bool IsAuthenticated { get; }
    Guid UserId { get; }
    bool HasPermission(string permission);
}