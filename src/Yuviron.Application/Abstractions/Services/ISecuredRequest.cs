using Yuviron.Domain.Enums;

namespace Yuviron.Application.Abstractions;

public interface ISecuredRequest
{
    AppPermission RequiredPermission { get; }
}