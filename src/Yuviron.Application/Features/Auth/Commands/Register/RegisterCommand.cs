using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Auth.Commands.Register;

public record RegisterCommand(
    string Email,
    string Password,
    string FirstName, 
    string Country,
    string City,
    DateTime DateOfBirth, 
    Gender Gender,    
    bool AcceptMarketing, 
    bool AcceptTerms,
    bool IsArtist   
) : IRequest<Guid>, ISensitiveRequest;