using FluentValidation;

namespace Yuviron.Application.Features.Files.Commands.DeleteFile;

public sealed class DeleteFileValidator : AbstractValidator<DeleteFileCommand>
{
    public DeleteFileValidator()
    {
        RuleFor(x => x.FilePath)
            .NotEmpty()
            .WithMessage("Путь к файлу не может быть пустым.")
            .MinimumLength(5)
            .WithMessage("Путь к файлу слишком короткий."); // Защита от случайного удаления корня ("/", "a")
    }
}