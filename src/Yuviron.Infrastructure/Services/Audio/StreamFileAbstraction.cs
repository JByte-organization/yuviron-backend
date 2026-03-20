using System;
using System.IO;

namespace Yuviron.Infrastructure.Services.Audio;

public class StreamFileAbstraction : TagLib.File.IFileAbstraction
{
    private readonly Stream _stream;

    public StreamFileAbstraction(string name, Stream stream)
    {
        Name = name;
        _stream = stream;
    }

    public string Name { get; }
    public Stream ReadStream => _stream;
    public Stream WriteStream => _stream;
    public void CloseStream(Stream stream) { }
}