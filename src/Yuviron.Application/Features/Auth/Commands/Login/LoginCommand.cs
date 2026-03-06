using MediatR;
using Yuviron.Application.Abstractions;

namespace Yuviron.Application.Features.Auth.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<LoginResponse>, ISensitiveRequest;