using System.Linq;
using FluentValidation;

namespace Yuviron.Application.Features.Files.Commands.UploadFile;

public sealed class UploadFileValidator : AbstractValidator<UploadFileCommand>
{
    private static readonly string[] AllowedFolders = { "avatars", "covers", "tracks", "temp", "uploads" };

    public UploadFileValidator()
    {
        RuleFor(x => x.FileStream)
            .NotNull()
            .WithMessage("The file cannot be empty.");

        RuleFor(x => x.FileName)
            .NotEmpty()
            .MaximumLength(255)
            .WithMessage("The file name is required and must not exceed 255 characters.");

        RuleFor(x => x.ContentType)
            .NotEmpty();

        RuleFor(x => x.Folder)
            .NotEmpty()
            .Must(BeAnAllowedFolder)
            .WithMessage($"Invalid destination folder. Allowed: {string.Join(", ", AllowedFolders)}.");
    }

    private static bool BeAnAllowedFolder(string folder)
    {
        if (string.IsNullOrWhiteSpace(folder)) return false;
        
        return AllowedFolders.Contains(folder.ToLower().Trim());
    }
}