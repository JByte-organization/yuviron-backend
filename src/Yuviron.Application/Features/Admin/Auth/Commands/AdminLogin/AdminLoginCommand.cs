using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Features.Auth.Commands.Login;

namespace Yuviron.Application.Features.Admin.Auth.Commands.AdminLogin;

public sealed record AdminLoginCommand(string Email, string Code) : IRequest<LoginResponse>, ISensitiveRequest;