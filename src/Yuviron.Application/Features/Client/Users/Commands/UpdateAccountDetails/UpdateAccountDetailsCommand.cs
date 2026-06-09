using MediatR;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Client.Users.Commands.UpdateAccountDetails;

public sealed record UpdateAccountDetailsCommand(
    string Email,
    string? Country,
    DateTime DateOfBirth,
    Gender Gender,
    bool AcceptMarketing) : IRequest<Unit>;
