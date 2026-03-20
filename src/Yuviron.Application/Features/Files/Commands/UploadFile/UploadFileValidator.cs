using FluentValidation;

namespace Yuviron.Application.Features.Files.Commands.UploadFile;

public sealed class UploadFileValidator : AbstractValidator<UploadFileCommand>
{
    public UploadFileValidator()
    {
        RuleFor(x => x.FileStream)
            .NotNull()
            .WithMessage("The file cannot be empty.");

        RuleFor(x => x.FileName)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(x => x.ContentType)
            .NotEmpty();
    }
}