using System;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Tracks.Commands.DTOs;

public record StudioTrackCollaboratorInput(Guid ArtistId, ArtistRole Role);