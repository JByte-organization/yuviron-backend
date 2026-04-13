using FluentValidation;

namespace Yuviron.Application.Features.Admin.Jamendo.Commands.SyncTracks;

public sealed class SyncJamendoTracksValidator : AbstractValidator<SyncJamendoTracksCommand>
{
    public SyncJamendoTracksValidator()
    {
        RuleFor(x => x.Limit)
            .GreaterThan(0)
            .WithMessage("Количество треков для синхронизации должно быть больше нуля.")
            .LessThanOrEqualTo(50)
            .WithMessage("Нельзя запрашивать более 50 треков за раз, чтобы не перегрузить сервер и API Jamendo.");
    }
}