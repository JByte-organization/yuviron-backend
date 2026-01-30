using MediatR;

namespace Yuviron.Application.Features.Auth.Commands.Register;

public record RegisterCommand(string Email, string Password, string FirstName) : IRequest<Guid>;