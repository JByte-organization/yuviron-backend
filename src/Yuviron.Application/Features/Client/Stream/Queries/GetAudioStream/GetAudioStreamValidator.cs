using FluentValidation;
using System;
using System.IO;

namespace Yuviron.Application.Features.Client.Stream.Queries.GetAudioStream;

public sealed class GetAudioStreamValidator : AbstractValidator<GetAudioStreamQuery>
{
    public GetAudioStreamValidator()
    {
        RuleFor(x => x.TrackId)
            .NotEqual(Guid.Empty);

        RuleFor(x => x.FileName)
            .NotEmpty()
            .MaximumLength(255)
            .Must(BeValidHlsFileName)
            .WithMessage("Invalid stream file requested. Only HLS segments and playlists are allowed.");
    }

    private bool BeValidHlsFileName(string fileName)
    {
        if (string.Equals(fileName, "key", StringComparison.OrdinalIgnoreCase)) 
            return true;

        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        
        if (ext != ".m3u8" && ext != ".ts") 
            return false;

        var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
        return !string.IsNullOrWhiteSpace(nameWithoutExt) && 
               !nameWithoutExt.Contains('/') && 
               !nameWithoutExt.Contains('\\') &&
               !nameWithoutExt.Contains("..");
    }
}