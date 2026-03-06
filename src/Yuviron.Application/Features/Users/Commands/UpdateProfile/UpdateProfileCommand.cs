using MediatR;
using Yuviron.Domain.Enums;

public sealed record UpdateProfileCommand(
    string DisplayName,
    DateTime DateOfBirth,
    Gender Gender,
    string? Bio,
    string? Country
) : IRequest<Unit>;