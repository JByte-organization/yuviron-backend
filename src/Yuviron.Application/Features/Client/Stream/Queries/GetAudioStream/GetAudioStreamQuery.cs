using System;
using MediatR;

namespace Yuviron.Application.Features.Client.Stream.Queries.GetAudioStream;

public sealed record GetAudioStreamQuery(
    Guid TrackId, 
    string FileName, 
    long Exp, 
    string Sig
) : IRequest<GetAudioStreamResponse>;