using FluentValidation;

namespace Yuviron.Application.Features.Files.Commands.UploadFile;

public sealed class UploadFileValidator : AbstractValidator<UploadFileCommand>
{
    // Whitelist of allowed extensions
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".mp3", ".wav" };
    
    // Whitelist of allowed MIME types
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
}