using System;
using MediatR;

namespace Yuviron.Application.Features.Client.Stream.Queries.GetAudioStream;

public sealed record GetAudioStreamQuery(
    Guid TrackId, 
    int Quality,  
    string FileName, 
    long Exp, 
    string Sig
) : IRequest<GetAudioStreamResponse>;