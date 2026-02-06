namespace Yuviron.Application.Abstractions;

public interface ISecuredRequest
{
    string RequiredPermission { get; }
}