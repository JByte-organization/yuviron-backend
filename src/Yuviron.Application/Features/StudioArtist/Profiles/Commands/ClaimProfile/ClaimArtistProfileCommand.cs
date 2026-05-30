using MediatR;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.ArtistDashboard.Profiles.Commands.ClaimProfile;

public sealed record ClaimArtistProfileRequest(
    ClaimRole ClaimedRole,
    string OfficialEmail,
    string Links, 
    Guid? ProofFileId,
    string? Message    
);

public sealed record ClaimArtistProfileCommand(
    Guid ArtistId,
    ClaimRole ClaimedRole,
    string OfficialEmail,
    string Links,
    Guid? ProofFileId,
    string? Message
) : IRequest;