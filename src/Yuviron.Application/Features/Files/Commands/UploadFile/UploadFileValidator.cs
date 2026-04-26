using System.IO;
using System.Linq;
using FluentValidation;

namespace Yuviron.Application.Features.Files.Commands.UploadFile;

public sealed class UploadFileValidator : AbstractValidator<UploadFileCommand>
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".mp3", ".wav" };
    private static readonly string[] AllowedMimeTypes = { "image/jpeg", "image/png", "image/webp", "audio/mpeg", "audio/wav", "audio/x-wav" };

    public UploadFileValidator()
    {
        RuleFor(x => x.FileStream)
            .NotNull().WithMessage("The file cannot be empty.")
            .Must(stream => stream.Length > 0).WithMessage("The file cannot be empty.")
            .Must(stream => stream.Length <= 50 * 1024 * 1024).WithMessage("File size must not exceed 50 MB.");

        RuleFor(x => x.FileName)
            .NotEmpty()
            .MaximumLength(255)
            .Must(HaveAllowedExtension).WithMessage($"Dangerous file type. Only the following extensions are allowed: {string.Join(", ", AllowedExtensions)}");

        RuleFor(x => x.ContentType)
            .NotEmpty()
            .Must(HaveAllowedMimeType).WithMessage("Invalid or dangerous content type.");

        RuleFor(x => x)
            .Must(x => HaveValidSignature(x.FileStream, x.FileName))
            .WithMessage("File content does not match its extension (Invalid Magic Bytes). Fake file detected!")
            .When(x => HaveAllowedExtension(x.FileName)); 
    }

    private bool HaveAllowedExtension(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName)) return false;
        
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return AllowedExtensions.Contains(extension);
    }

    private bool HaveAllowedMimeType(string contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType)) return false;
        
        return AllowedMimeTypes.Contains(contentType.ToLowerInvariant());
    }

    private bool HaveValidSignature(Stream stream, string fileName)
    {
        if (stream == null || !stream.CanRead || !stream.CanSeek) return false;

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        
        byte[] headerBytes = new byte[12];
        
        stream.Position = 0;
        stream.Read(headerBytes, 0, 12);
        
        stream.Position = 0;

        if (headerBytes.Length < 4) return false;

        return extension switch
        {
            ".jpg" or ".jpeg" => 
                headerBytes.Take(3).SequenceEqual(new byte[] { 0xFF, 0xD8, 0xFF }),
                
            ".png" => 
                headerBytes.Take(8).SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
                
            ".mp3" => 
                headerBytes.Take(3).SequenceEqual(new byte[] { 0x49, 0x44, 0x33 }) || 
                (headerBytes[0] == 0xFF && (headerBytes[1] == 0xFB || headerBytes[1] == 0xF3 || headerBytes[1] == 0xF2)),
                
            ".webp" => 
                headerBytes.Take(4).SequenceEqual(new byte[] { 0x52, 0x49, 0x46, 0x46 }) && // RIFF
                headerBytes.Skip(8).Take(4).SequenceEqual(new byte[] { 0x57, 0x45, 0x42, 0x50 }), // WEBP
                
            ".wav" => 
                headerBytes.Take(4).SequenceEqual(new byte[] { 0x52, 0x49, 0x46, 0x46 }) && // RIFF
                headerBytes.Skip(8).Take(4).SequenceEqual(new byte[] { 0x57, 0x41, 0x56, 0x45 }), // WAVE
                
            _ => false
        };
    }
}