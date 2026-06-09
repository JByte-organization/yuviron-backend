using System;
using System.Collections.Generic;
using System.Linq;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Client.Notifications.Preferences;

public static class NotificationPreferenceCatalog
{
    public const string CategoryAllCode = "all";

    private static readonly IReadOnlyList<NotificationPreferenceGroupDefinition> Groups =
        new[]
        {
            new NotificationPreferenceGroupDefinition(
                NotificationCategory.System,
                "System",
                new[]
                {
                    new NotificationPreferenceItemDefinition(NotificationCategory.System, CategoryAllCode, "All system notifications", true, true),
                    new NotificationPreferenceItemDefinition(NotificationCategory.System, "new_device_login", "New device login", true),
                    new NotificationPreferenceItemDefinition(NotificationCategory.System, "new_password_changed", "Password changed", true),
                    new NotificationPreferenceItemDefinition(NotificationCategory.System, "payout_settings_changed", "Payout settings changed", true),
                    new NotificationPreferenceItemDefinition(NotificationCategory.System, "team_joined", "Team member joined", true),
                    new NotificationPreferenceItemDefinition(NotificationCategory.System, "team_member_removed", "Team member removed", true),
                    new NotificationPreferenceItemDefinition(NotificationCategory.System, "team_role_changed", "Team role changed", true),
                    new NotificationPreferenceItemDefinition(NotificationCategory.System, "track_processed", "Track processed", true),
                    new NotificationPreferenceItemDefinition(NotificationCategory.System, "moderated_track_deleted", "Moderated track deleted", true),
                    new NotificationPreferenceItemDefinition(NotificationCategory.System, "moderated_album_deleted", "Moderated album deleted", true),
                    new NotificationPreferenceItemDefinition(NotificationCategory.System, "moderated_playlist_deleted", "Moderated playlist deleted", true),
                    new NotificationPreferenceItemDefinition(NotificationCategory.System, "banner_request_approved", "Banner request approved", true),
                    new NotificationPreferenceItemDefinition(NotificationCategory.System, "banner_request_paid", "Banner request paid", true),
                    new NotificationPreferenceItemDefinition(NotificationCategory.System, "banner_request_rejected", "Banner request rejected", true),
                    new NotificationPreferenceItemDefinition(NotificationCategory.System, "complaint_approved", "Complaint approved", true),
                    new NotificationPreferenceItemDefinition(NotificationCategory.System, "complaint_rejected", "Complaint rejected", true),
                    new NotificationPreferenceItemDefinition(NotificationCategory.System, "artist_claim_approved", "Artist claim approved", true),
                    new NotificationPreferenceItemDefinition(NotificationCategory.System, "artist_claim_rejected", "Artist claim rejected", true)
                }),
            new NotificationPreferenceGroupDefinition(
                NotificationCategory.Music,
                "Music",
                new[]
                {
                    new NotificationPreferenceItemDefinition(NotificationCategory.Music, CategoryAllCode, "All music notifications", true, true),
                    new NotificationPreferenceItemDefinition(NotificationCategory.Music, "new_release", "New release", true),
                    new NotificationPreferenceItemDefinition(NotificationCategory.Music, "editorial_playlist", "Editorial playlist", true),
                    new NotificationPreferenceItemDefinition(NotificationCategory.Music, "track_trending", "Track trending", true),
                    new NotificationPreferenceItemDefinition(NotificationCategory.Music, "track_entered_top_chart", "Track entered top chart", true),
                    new NotificationPreferenceItemDefinition(NotificationCategory.Music, "track_play_milestone", "Track play milestone", true),
                    new NotificationPreferenceItemDefinition(NotificationCategory.Music, "track_processed", "Track processed", true),
                    new NotificationPreferenceItemDefinition(NotificationCategory.Music, "track_lyrics_updated", "Track lyrics updated", true),
                    new NotificationPreferenceItemDefinition(NotificationCategory.Music, "studio_playlist_track_changed", "Studio playlist track changed", true)
                }),
            new NotificationPreferenceGroupDefinition(
                NotificationCategory.Social,
                "Social",
                new[]
                {
                    new NotificationPreferenceItemDefinition(NotificationCategory.Social, CategoryAllCode, "All social notifications", true, true),
                    new NotificationPreferenceItemDefinition(NotificationCategory.Social, "new_follower", "New follower", true),
                    new NotificationPreferenceItemDefinition(NotificationCategory.Social, "artist_followers_milestone", "Artist followers milestone", true),
                    new NotificationPreferenceItemDefinition(NotificationCategory.Social, "playlist_favorited", "Playlist favorited", true)
                }),
            new NotificationPreferenceGroupDefinition(
                NotificationCategory.Billing,
                "Billing",
                new[]
                {
                    new NotificationPreferenceItemDefinition(NotificationCategory.Billing, CategoryAllCode, "All billing notifications", true, true),
                    new NotificationPreferenceItemDefinition(NotificationCategory.Billing, "subscription_activated", "Subscription activated", true),
                    new NotificationPreferenceItemDefinition(NotificationCategory.Billing, "subscription_canceled", "Subscription canceled", true),
                    new NotificationPreferenceItemDefinition(NotificationCategory.Billing, "subscription_renewed", "Subscription renewed", true),
                    new NotificationPreferenceItemDefinition(NotificationCategory.Billing, "subscription_payment_failed", "Subscription payment failed", true),
                    new NotificationPreferenceItemDefinition(NotificationCategory.Billing, "first_royalties", "First royalties", true),
                    new NotificationPreferenceItemDefinition(NotificationCategory.Billing, "payout_requested", "Payout requested", true),
                    new NotificationPreferenceItemDefinition(NotificationCategory.Billing, "payout_approved", "Payout approved", true),
                    new NotificationPreferenceItemDefinition(NotificationCategory.Billing, "payout_rejected", "Payout rejected", true),
                    new NotificationPreferenceItemDefinition(NotificationCategory.Billing, "artist_subscription_activated", "Artist subscription activated", true),
                    new NotificationPreferenceItemDefinition(NotificationCategory.Billing, "artist_subscription_canceled", "Artist subscription canceled", true),
                    new NotificationPreferenceItemDefinition(NotificationCategory.Billing, "artist_subscription_payment_failed", "Artist subscription payment failed", true)
                })
        };

    public static IReadOnlyList<NotificationPreferenceGroupDefinition> GetGroups() => Groups;

    public static NotificationPreferenceItemDefinition? Find(NotificationCategory category, string code)
        => Groups.SelectMany(group => group.Items).FirstOrDefault(x =>
            x.Category == category && x.Code.Equals(code, StringComparison.OrdinalIgnoreCase));

    public static bool IsDefaultEnabled(NotificationCategory category, string code)
        => Find(category, code)?.DefaultEnabled ?? true;
}

public sealed record NotificationPreferenceGroupDefinition(
    NotificationCategory Category,
    string Title,
    IReadOnlyList<NotificationPreferenceItemDefinition> Items);

public sealed record NotificationPreferenceItemDefinition(
    NotificationCategory Category,
    string Code,
    string Title,
    bool DefaultEnabled,
    bool IsCategoryDefault = false);
