using MediatR;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Auth.Commands.Register;

public record RegisterCommand(
    string Email,
    string Password,
    string FirstName, 
    DateTime DateOfBirth, 
    Gender Gender,    
    bool AcceptMarketing, 
    bool AcceptTerms    
) : IRequest<Guid>;