using MediatR;
using System;

namespace Yuviron.Application.Features.Client.Tracks.Queries.GetTrackLyrics;

public sealed record GetTrackLyricsQuery(Guid TrackId) : IRequest<TrackLyricsDto>;