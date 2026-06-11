using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.StudioArtist.Profile.Commands.AddSocialLink;

public sealed class AddSocialLinkHandler : IRequestHandler<AddSocialLinkCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly TimeProvider _timeProvider;

    public AddSocialLinkHandler(IApplicationDbContext context, ICurrentUserService currentUser, TimeProvider timeProvider)
    {
        _context = context; _currentUser = currentUser; _timeProvider = timeProvider;
    }

    public async Task<Unit> Handle(AddSocialLinkCommand request, CancellationToken cancellationToken)
    {
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        // ВАЖНО: Мы не делаем .Include(a => a.SocialLinks)
        // Мы не трогаем сущность Artist вообще, чтобы не провоцировать UPDATE
    
        // Сразу создаем запись в таблицу связей
        var newLink = ArtistSocialLink.Create(request.ArtistId, request.Type, request.Url, utcNow);
    
        _context.ArtistSocialLinks.Add(newLink);

        // Это сделает обычный INSERT, который не вызывает конфликтов параллелизма
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}