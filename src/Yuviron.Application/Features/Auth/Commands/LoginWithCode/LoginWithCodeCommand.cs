using MediatR;
using Yuviron.Application.Features.Auth.Commands.Login; // Ссылка на LoginResponse

namespace Yuviron.Application.Features.Auth.Commands.LoginWithCode;

public record LoginWithCodeCommand(string Email, string Code) : IRequest<LoginResponse>;