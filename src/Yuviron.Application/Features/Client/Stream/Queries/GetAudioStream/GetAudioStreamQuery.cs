using System;
using MediatR;

namespace Yuviron.Application.Features.Client.Stream.Queries.GetAudioStream;

public sealed record GetAudioStreamQuery(
    Guid TrackId,
    int Quality,
    string FileName,
    long Exp,
    Guid Uid,
    string Sig,
    // Captured for anomaly-detection logging only — NOT part of the signature payload.
    // Binding the HMAC to these broke legitimate playback on network/token churn (see PR #118).
    string IpAddress,
    string UserAgent
) : IRequest<GetAudioStreamResponse>;