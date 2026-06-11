using System;
using MediatR;

namespace Yuviron.Application.Features.Admin.Tracks.Queries.StreamAdminTrackAudio;

public sealed record StreamAdminTrackAudioQuery(
    Guid TrackId,
    long Exp,
    Guid Uid,
    string Sig
) : IRequest<StreamAdminTrackAudioResponse>;
