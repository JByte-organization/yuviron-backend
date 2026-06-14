using Yuviron.Infrastructure.Persistence;
using Yuviron.Application.Abstractions.Data.Contexts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;
using Yuviron.Infrastructure.Consumers.Bases;

namespace Yuviron.Infrastructure.Consumers;

public class NotifyStudioTeamOnPlaylistAdditionConsumer : NotifyArtistTeamConsumerBase<TrackAddedToEditorialPlaylistEvent>
{
    public NotifyStudioTeamOnPlaylistAdditionConsumer(AppDbContext context, INotificationService notificationService) : base(context, notificationService) { }

    protected override async Task SendNotificationAsync(TrackAddedToEditorialPlaylistEvent msg, List<Guid> teamIds, CancellationToken ct) =>
        await NotificationService.SendToUsersAsync(teamIds, NotificationCategory.Music, "editorial_playlist", 
            "Успіх редакції! 🌟", $"Ваш трек «{msg.TrackTitle}» потрапив в офіційний плейлист Yuviron «{msg.PlaylistName}».", NotificationEntityType.Track, msg.TrackId, ct);
}