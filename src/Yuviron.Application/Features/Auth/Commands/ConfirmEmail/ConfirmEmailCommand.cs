using MediatR;
using Yuviron.Application.Abstractions;

namespace Yuviron.Application.Features.Auth.Commands.ConfirmEmail;

public record ConfirmEmailCommand(string Token) : IRequest<Unit>, ISensitiveRequest;