using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Features.Auth.Commands.Login; 

namespace Yuviron.Application.Features.Auth.Commands.LoginWithCode;

public record LoginWithCodeCommand(string Email, string Code) : IRequest<LoginResponse>, ISensitiveRequest;