using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Features.Auth.Commands.Login; 
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Auth.Commands.Register;

public record RegisterCommand(
    string Email,
    string Password,
    string FirstName, 
    DateTime DateOfBirth, 
    Gender Gender,    
    bool AcceptMarketing, 
    bool AcceptTerms,
    bool IsArtist,
    string? ArtistName    
) : IRequest<LoginResponse>, ISensitiveRequest;