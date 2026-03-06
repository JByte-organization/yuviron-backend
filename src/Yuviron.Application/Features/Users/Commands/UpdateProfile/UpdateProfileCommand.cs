using MediatR;
using Yuviron.Domain.Enums;
namespace Yuviron.Application.Features.Users.Commands.UpdateProfile;
public sealed record UpdateProfileCommand(
    string DisplayName,
    DateTime DateOfBirth,
    Gender Gender,
    string? Bio,
    string? Country
) : IRequest<Unit>;