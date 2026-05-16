using MediatR;
using Yuviron.Application.Abstractions;

namespace Yuviron.Application.Features.Admin.Auth.Commands.AdminPreLogin;

public sealed record AdminPreLoginCommand(string Email, string Password) : IRequest<Unit>, ISensitiveRequest;