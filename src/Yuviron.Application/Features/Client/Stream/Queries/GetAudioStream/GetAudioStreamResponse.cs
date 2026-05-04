using System.IO;

namespace Yuviron.Application.Features.Client.Stream.Queries.GetAudioStream;

public sealed record GetAudioStreamResponse(
    System.IO.Stream Stream, 
    string ContentType
);