using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Enums.Monetization;
using Yuviron.Domain.Events; 

namespace Yuviron.Application.Features.Finance.Commands.CalculateDailyRoyalties;

public sealed class CalculateDailyRoyaltiesHandler : IRequestHandler<CalculateDailyRoyaltiesCommand>
{
    private readonly IMonetizationContext _monetizationContext;
    private readonly ISystemContext _systemContext;
    private readonly ILogger<CalculateDailyRoyaltiesHandler> _logger;
    private readonly TimeProvider _timeProvider;
    private readonly IEventBus _eventBus; 

    private const decimal GlobalRatePerStream = 0.003m; 

    public CalculateDailyRoyaltiesHandler(
        IMonetizationContext monetizationContext, ISystemContext systemContext, 
        ILogger<CalculateDailyRoyaltiesHandler> logger, 
        TimeProvider timeProvider,
        IEventBus eventBus) 
    {
        _monetizationContext = monetizationContext;
        _systemContext = systemContext; 
        _logger = logger; 
        _timeProvider = timeProvider;
        _eventBus = eventBus;
    }

    public async Task Handle(CalculateDailyRoyaltiesCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting daily royalty calculation for {Date}", request.TargetDate);
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var startDate = request.TargetDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var endDate = startDate.AddDays(1);

        var artistStreamCounts = await _systemContext.ListeningEvents
            .AsNoTracking()
            .Where(e => e.PlayedAt >= startDate && e.PlayedAt < endDate && e.MsPlayed >= 30000)
            .SelectMany(e => e.Track.TrackArtists.Where(ta => ta.Role == ArtistRole.Main)) 
            .GroupBy(ta => ta.ArtistId)
            .Select(g => new { ArtistId = g.Key, ValidStreams = g.Count() })
            .ToListAsync(cancellationToken);

        if (!artistStreamCounts.Any())
        {
            _logger.LogInformation("No valid streams found for {Date}. Skipping.", request.TargetDate);
            return;
        }

        var artistIds = artistStreamCounts.Select(x => x.ArtistId).ToList();

        var settingsDict = await _monetizationContext.ArtistPayoutSettings
            .Where(s => artistIds.Contains(s.ArtistId))
            .ToDictionaryAsync(s => s.ArtistId, cancellationToken);

        var walletsDict = await _monetizationContext.ArtistWallets
            .Where(w => artistIds.Contains(w.ArtistId))
            .ToDictionaryAsync(w => w.ArtistId, cancellationToken);

        var existingAccruals = await _monetizationContext.RoyaltyAccrualsDaily
            .Where(a => artistIds.Contains(a.ArtistId) && a.Date == request.TargetDate)
            .Select(a => a.ArtistId)
            .ToListAsync(cancellationToken);
            
        var existingAccrualSet = new HashSet<Guid>(existingAccruals);

        var eventsToPublish = new List<FirstRoyaltiesEarnedEvent>();

        foreach (var stats in artistStreamCounts)
        {
            if (existingAccrualSet.Contains(stats.ArtistId))
            {
                _logger.LogWarning("Accrual for artist {ArtistId} on {Date} already exists. Skipping.", stats.ArtistId, request.TargetDate);
                continue;
            }

            var settings = settingsDict.GetValueOrDefault(stats.ArtistId);
            var rate = settings?.CustomRatePerStream ?? GlobalRatePerStream;
            var platformPercent = settings?.PlatformPercent ?? 30;

            decimal grossAmount = stats.ValidStreams * rate;
            decimal platformFeeAmount = grossAmount * (platformPercent / 100m);
            decimal netAmount = grossAmount - platformFeeAmount; 

            if (netAmount <= 0) continue;

            var accrual = RoyaltyAccrualDaily.Create(stats.ArtistId, request.TargetDate, stats.ValidStreams, grossAmount, platformFeeAmount);
            _monetizationContext.Add(accrual);

            bool isFirstRoyalty = false;

            if (!walletsDict.TryGetValue(stats.ArtistId, out var wallet))
            {
                wallet = ArtistWallet.Create(stats.ArtistId, utcNow);
                _monetizationContext.Add(wallet);
                isFirstRoyalty = true; 
            }
            else if (wallet.TotalEarned == 0) 
            {
                isFirstRoyalty = true; 
            }

            if (isFirstRoyalty)
            {
                eventsToPublish.Add(new FirstRoyaltiesEarnedEvent(stats.ArtistId));
            }
            
            wallet.CreditRoyalties(netAmount, utcNow);

            var walletTx = WalletTransaction.Create(
                wallet.Id, netAmount, WalletTransactionType.RoyaltyAccrual, 
                $"Daily Royalties: {stats.ValidStreams} streams", null, utcNow);
            
            _monetizationContext.Add(walletTx);
        }

        await _monetizationContext.SaveChangesAsync(cancellationToken);

        foreach (var evt in eventsToPublish)
        {
            await _eventBus.PublishAsync(evt, cancellationToken);
        }

        _logger.LogInformation("Successfully calculated royalties for {Count} artists.", artistStreamCounts.Count);
    }
}