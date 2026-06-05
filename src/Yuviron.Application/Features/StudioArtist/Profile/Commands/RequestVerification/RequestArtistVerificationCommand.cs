using MediatR;
using System;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Profile.Commands.RequestVerification;

public sealed record RequestArtistVerificationCommand(
    Guid ArtistId,
    string OfficialEmail,
    string Links,
    Guid? ProofFileId,
    string? Message
) : IRequest<Unit>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.StudioArtistManage; 
}