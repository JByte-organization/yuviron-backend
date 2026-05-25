
using FluentValidation;
using SkiaSharp;
using Yuviron.Application.Common.Utilities;

namespace Yuviron.Application.Features.Files.Commands.UploadFile;

public sealed class UploadFileValidator : AbstractValidator<UploadFileCommand>
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".mp3", ".wav" };
    
    private static readonly string[] AllowedMimeTypes = { "image/jpeg", "image/png", "image/webp", "audio/mpeg", "audio/wav", "audio/x-wav" };

    public UploadFileValidator()
    {
        RuleFor(x => x.FileStream)
            .Cascade(CascadeMode.Stop)
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
        
        RuleFor(x => x)
            .Must(x => IsImageValidDimensions(x.FileStream, x.FileName))
            .WithMessage("Image dimensions are invalid (too large or too small).")
            .When(x => IsImageExtension(x.FileName));
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
        var expectedExt = Path.GetExtension(fileName).ToLowerInvariant();
        
        
        if (expectedExt == ".jpeg") expectedExt = ".jpg";

        var (detectedExt, _) = FileSignatureDetector.Detect(stream);

        return detectedExt == expectedExt;
    }
    
    private bool IsImageExtension(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        
        return ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".webp";
    }

    private bool IsImageValidDimensions(Stream stream, string fileName)
    {
        try
        {
            using var managedStream = new SKManagedStream(stream, disposeManagedStream: false);
        
            using var codec = SKCodec.Create(managedStream);
    
            if (codec == null) return false;

            return codec.Info.Width >= 150 && codec.Info.Width <= 4000 && 
                   codec.Info.Height >= 150 && codec.Info.Height <= 4000;
        }
        catch
        {
            return false;
        }
        finally
        {
            if (stream != null && stream.CanSeek)
            {
                stream.Position = 0; 
            }
        }
    }
}