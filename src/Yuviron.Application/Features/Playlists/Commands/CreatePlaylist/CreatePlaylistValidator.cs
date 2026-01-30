using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yuviron.Application.Features.Auth.Commands.Register;

namespace Yuviron.Application.Features.Playlists.Commands.CreatePlaylist
{
    public class CreatePlaylistValidator : AbstractValidator<CreatePlaylistCommand>
    {
        public CreatePlaylistValidator() 
        {
            RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Название обязательно.")
            .MaximumLength(100).WithMessage("Максимум 100 символов.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Максимум 500 символов.");

            When(x => !string.IsNullOrEmpty(x.CoverUrl), () =>
            {
                RuleFor(x => x.CoverUrl)
                    .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
                    .WithMessage("Некорректный формат URL обложки.");
            });
        }
    }
}
