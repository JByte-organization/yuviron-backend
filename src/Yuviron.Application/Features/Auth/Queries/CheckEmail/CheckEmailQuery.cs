using MediatR;

namespace Yuviron.Application.Features.Auth.Queries.CheckEmail;

public record CheckEmailQuery(string Email) : IRequest<bool>;