CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;
ALTER DATABASE CHARACTER SET utf8mb4;

CREATE TABLE `achievements` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `Code` varchar(64) CHARACTER SET utf8mb4 NOT NULL,
    `Title` varchar(128) CHARACTER SET utf8mb4 NOT NULL,
    `Description` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
    `IsActive` tinyint(1) NOT NULL,
    CONSTRAINT `PK_achievements` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `ads` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `Title` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `MediaUrl` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
    `IsActive` tinyint(1) NOT NULL,
    CONSTRAINT `PK_ads` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `albums` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `Title` varchar(256) CHARACTER SET utf8mb4 NOT NULL,
    `Description` varchar(2000) CHARACTER SET utf8mb4 NULL,
    `CoverUrl` varchar(500) CHARACTER SET utf8mb4 NULL,
    `ReleaseDate` datetime(6) NOT NULL,
    `VisibilityStatus` int NOT NULL,
    `ScheduledPublishAt` datetime(6) NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NOT NULL,
    `IsDeleted` tinyint(1) NOT NULL,
    `DeletedAt` datetime(6) NULL,
    CONSTRAINT `PK_albums` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `artists` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `Name` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `Bio` varchar(2000) CHARACTER SET utf8mb4 NULL,
    `AvatarUrl` varchar(500) CHARACTER SET utf8mb4 NULL,
    `BannerUrl` varchar(500) CHARACTER SET utf8mb4 NULL,
    `IsVerified` tinyint(1) NOT NULL,
    `VerificationStatus` int NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NOT NULL,
    `IsDeleted` tinyint(1) NOT NULL,
    `DeletedAt` datetime(6) NULL,
    CONSTRAINT `PK_artists` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `complaint_counters` (
    `TargetType` int NOT NULL,
    `TargetId` char(36) COLLATE ascii_general_ci NOT NULL,
    `CountOpen` int NOT NULL,
    `CountTotal` int NOT NULL,
    `UpdatedAt` datetime(6) NOT NULL,
    CONSTRAINT `PK_complaint_counters` PRIMARY KEY (`TargetType`, `TargetId`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `genres` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `Name` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `CoverUrl` varchar(2000) CHARACTER SET utf8mb4 NULL,
    `HexColor` longtext CHARACTER SET utf8mb4 NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NOT NULL,
    `IsDeleted` tinyint(1) NOT NULL,
    CONSTRAINT `PK_genres` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `moods` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `Name` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `CoverUrl` varchar(2000) CHARACTER SET utf8mb4 NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NOT NULL,
    `IsDeleted` tinyint(1) NOT NULL,
    CONSTRAINT `PK_moods` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `permissions` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `Name` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `Description` varchar(250) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK_permissions` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `plans` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `Name` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `Price` decimal(18,2) NOT NULL,
    `Currency` char(3) CHARACTER SET utf8mb4 NOT NULL,
    `Period` int NOT NULL,
    CONSTRAINT `PK_plans` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `roles` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `Name` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK_roles` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `themes` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `Name` varchar(64) CHARACTER SET utf8mb4 NOT NULL,
    `IsSystem` tinyint(1) NOT NULL,
    `IsPremiumOnly` tinyint(1) NOT NULL,
    CONSTRAINT `PK_themes` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `users` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `Email` varchar(320) CHARACTER SET utf8mb4 NOT NULL,
    `PasswordHash` varchar(256) CHARACTER SET utf8mb4 NOT NULL,
    `AccountState` int NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NOT NULL,
    `LastLoginAt` datetime(6) NULL,
    `AcceptMarketing` tinyint(1) NOT NULL DEFAULT FALSE,
    `AcceptTerms` tinyint(1) NOT NULL,
    `LoginCodeHash` varchar(256) CHARACTER SET utf8mb4 NULL,
    `LoginCodeExpiryUtc` datetime(6) NULL,
    `IsDeleted` tinyint(1) NOT NULL,
    `DeletedAt` datetime(6) NULL,
    CONSTRAINT `PK_users` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `tracks` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `AlbumId` char(36) COLLATE ascii_general_ci NULL,
    `Title` varchar(256) CHARACTER SET utf8mb4 NOT NULL,
    `DurationMs` int NOT NULL,
    `Explicit` tinyint(1) NOT NULL,
    `CoverUrl` varchar(500) CHARACTER SET utf8mb4 NULL,
    `AudioStorageKey` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
    `PreviewStorageKey` varchar(500) CHARACTER SET utf8mb4 NULL,
    `VisibilityStatus` int NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NOT NULL,
    `IsDeleted` tinyint(1) NOT NULL,
    `DeletedAt` datetime(6) NULL,
    CONSTRAINT `PK_tracks` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_tracks_albums_AlbumId` FOREIGN KEY (`AlbumId`) REFERENCES `albums` (`Id`) ON DELETE SET NULL
) CHARACTER SET=utf8mb4;

CREATE TABLE `album_artists` (
    `AlbumId` char(36) COLLATE ascii_general_ci NOT NULL,
    `ArtistId` char(36) COLLATE ascii_general_ci NOT NULL,
    `Role` int NOT NULL,
    CONSTRAINT `PK_album_artists` PRIMARY KEY (`AlbumId`, `ArtistId`),
    CONSTRAINT `FK_album_artists_albums_AlbumId` FOREIGN KEY (`AlbumId`) REFERENCES `albums` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_album_artists_artists_ArtistId` FOREIGN KEY (`ArtistId`) REFERENCES `artists` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `artist_payout_settings` (
    `ArtistId` char(36) COLLATE ascii_general_ci NOT NULL,
    `MinWithdrawAmount` decimal(18,2) NOT NULL,
    `MaxWithdrawAmount` decimal(18,2) NOT NULL,
    `CustomRatePerStream` decimal(18,6) NULL,
    `PlatformPercent` int NOT NULL,
    CONSTRAINT `PK_artist_payout_settings` PRIMARY KEY (`ArtistId`),
    CONSTRAINT `FK_artist_payout_settings_artists_ArtistId` FOREIGN KEY (`ArtistId`) REFERENCES `artists` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `artist_pins` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `ArtistId` char(36) COLLATE ascii_general_ci NOT NULL,
    `EntityType` int NOT NULL,
    `EntityId` char(36) COLLATE ascii_general_ci NOT NULL,
    `Position` int NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    CONSTRAINT `PK_artist_pins` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_artist_pins_artists_ArtistId` FOREIGN KEY (`ArtistId`) REFERENCES `artists` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `artist_social_links` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `ArtistId` char(36) COLLATE ascii_general_ci NOT NULL,
    `Type` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
    `Url` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK_artist_social_links` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_artist_social_links_artists_ArtistId` FOREIGN KEY (`ArtistId`) REFERENCES `artists` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `copyright_claims` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `EntityType` int NOT NULL,
    `EntityId` char(36) COLLATE ascii_general_ci NOT NULL,
    `OwnerArtistId` char(36) COLLATE ascii_general_ci NOT NULL,
    `OwnsAllRights` tinyint(1) NOT NULL,
    `Notes` varchar(2000) CHARACTER SET utf8mb4 NULL,
    `CreatedAt` datetime(6) NOT NULL,
    CONSTRAINT `PK_copyright_claims` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_copyright_claims_artists_OwnerArtistId` FOREIGN KEY (`OwnerArtistId`) REFERENCES `artists` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `release_notification_templates` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `ArtistId` char(36) COLLATE ascii_general_ci NOT NULL,
    `TitleTemplate` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `BodyTemplate` varchar(1000) CHARACTER SET utf8mb4 NOT NULL,
    `IsEnabled` tinyint(1) NOT NULL,
    CONSTRAINT `PK_release_notification_templates` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_release_notification_templates_artists_ArtistId` FOREIGN KEY (`ArtistId`) REFERENCES `artists` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `royalty_accruals_daily` (
    `ArtistId` char(36) COLLATE ascii_general_ci NOT NULL,
    `Date` date NOT NULL,
    `StreamsCount` int NOT NULL,
    `GrossAmount` decimal(18,6) NOT NULL,
    `PlatformFeeAmount` decimal(18,6) NOT NULL,
    `NetAmount` decimal(18,6) NOT NULL,
    CONSTRAINT `PK_royalty_accruals_daily` PRIMARY KEY (`ArtistId`, `Date`),
    CONSTRAINT `FK_royalty_accruals_daily_artists_ArtistId` FOREIGN KEY (`ArtistId`) REFERENCES `artists` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `role_permissions` (
    `RoleId` char(36) COLLATE ascii_general_ci NOT NULL,
    `PermissionId` char(36) COLLATE ascii_general_ci NOT NULL,
    CONSTRAINT `PK_role_permissions` PRIMARY KEY (`RoleId`, `PermissionId`),
    CONSTRAINT `FK_role_permissions_permissions_PermissionId` FOREIGN KEY (`PermissionId`) REFERENCES `permissions` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_role_permissions_roles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `roles` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `ad_impressions` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `AdId` char(36) COLLATE ascii_general_ci NOT NULL,
    `UserId` char(36) COLLATE ascii_general_ci NULL,
    `ShownAt` datetime(6) NOT NULL,
    `Context` varchar(100) CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_ad_impressions` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_ad_impressions_ads_AdId` FOREIGN KEY (`AdId`) REFERENCES `ads` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_ad_impressions_users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE SET NULL
) CHARACTER SET=utf8mb4;

CREATE TABLE `artist_team_members` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `ArtistId` char(36) COLLATE ascii_general_ci NOT NULL,
    `UserId` char(36) COLLATE ascii_general_ci NOT NULL,
    `Role` int NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    CONSTRAINT `PK_artist_team_members` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_artist_team_members_artists_ArtistId` FOREIGN KEY (`ArtistId`) REFERENCES `artists` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_artist_team_members_users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `complaints` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `CreatedByUserId` char(36) COLLATE ascii_general_ci NOT NULL,
    `TargetType` int NOT NULL,
    `TargetId` char(36) COLLATE ascii_general_ci NOT NULL,
    `ReasonCode` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
    `Comment` varchar(2000) CHARACTER SET utf8mb4 NULL,
    `Status` int NOT NULL,
    `ModeratedByAdminId` char(36) COLLATE ascii_general_ci NULL,
    `ModerationNote` varchar(2000) CHARACTER SET utf8mb4 NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NOT NULL,
    CONSTRAINT `PK_complaints` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_complaints_users_CreatedByUserId` FOREIGN KEY (`CreatedByUserId`) REFERENCES `users` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_complaints_users_ModeratedByAdminId` FOREIGN KEY (`ModeratedByAdminId`) REFERENCES `users` (`Id`) ON DELETE SET NULL
) CHARACTER SET=utf8mb4;

CREATE TABLE `custom_themes` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `UserId` char(36) COLLATE ascii_general_ci NOT NULL,
    `PrimaryColor` char(7) CHARACTER SET utf8mb4 NOT NULL,
    `SecondaryColor` char(7) CHARACTER SET utf8mb4 NOT NULL,
    `BackgroundColor` char(7) CHARACTER SET utf8mb4 NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    CONSTRAINT `PK_custom_themes` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_custom_themes_users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `notifications` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `UserId` char(36) COLLATE ascii_general_ci NOT NULL,
    `Title` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `Body` varchar(1000) CHARACTER SET utf8mb4 NOT NULL,
    `EntityType` int NULL,
    `EntityId` char(36) COLLATE ascii_general_ci NULL,
    `IsRead` tinyint(1) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    CONSTRAINT `PK_notifications` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_notifications_users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `payout_requests` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `ArtistId` char(36) COLLATE ascii_general_ci NOT NULL,
    `RequestedAmount` decimal(18,2) NOT NULL,
    `Status` int NOT NULL,
    `RequestedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NOT NULL,
    `AdminId` char(36) COLLATE ascii_general_ci NULL,
    `DecisionNote` varchar(1000) CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_payout_requests` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_payout_requests_artists_ArtistId` FOREIGN KEY (`ArtistId`) REFERENCES `artists` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_payout_requests_users_AdminId` FOREIGN KEY (`AdminId`) REFERENCES `users` (`Id`) ON DELETE SET NULL
) CHARACTER SET=utf8mb4;

CREATE TABLE `playback_sessions` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `UserId` char(36) COLLATE ascii_general_ci NOT NULL,
    `StartedAt` datetime(6) NOT NULL,
    `EndedAt` datetime(6) NULL,
    `ContextType` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
    `ContextId` char(36) COLLATE ascii_general_ci NULL,
    CONSTRAINT `PK_playback_sessions` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_playback_sessions_users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `playlists` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `UserId` char(36) COLLATE ascii_general_ci NULL,
    `Title` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `Description` varchar(2000) CHARACTER SET utf8mb4 NULL,
    `CoverUrl` varchar(500) CHARACTER SET utf8mb4 NULL,
    `IsPublic` tinyint(1) NOT NULL,
    `IsEditorial` tinyint(1) NOT NULL,
    `IsDeleted` tinyint(1) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NOT NULL,
    CONSTRAINT `PK_playlists` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_playlists_users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `refresh_tokens` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `UserId` char(36) COLLATE ascii_general_ci NOT NULL,
    `TokenHash` varchar(512) CHARACTER SET utf8mb4 NOT NULL,
    `ExpiresAt` datetime(6) NOT NULL,
    `RevokedAt` datetime(6) NULL,
    `CreatedAt` datetime(6) NOT NULL,
    CONSTRAINT `PK_refresh_tokens` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_refresh_tokens_users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `shared_rooms` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `HostUserId` char(36) COLLATE ascii_general_ci NOT NULL,
    `Name` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `Status` int NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `EndedAt` datetime(6) NULL,
    CONSTRAINT `PK_shared_rooms` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_shared_rooms_users_HostUserId` FOREIGN KEY (`HostUserId`) REFERENCES `users` (`Id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;

CREATE TABLE `smart_links` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `Code` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    `EntityType` int NOT NULL,
    `EntityId` char(36) COLLATE ascii_general_ci NOT NULL,
    `CreatedByUserId` char(36) COLLATE ascii_general_ci NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `ExpiresAt` datetime(6) NULL,
    CONSTRAINT `PK_smart_links` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_smart_links_users_CreatedByUserId` FOREIGN KEY (`CreatedByUserId`) REFERENCES `users` (`Id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;

CREATE TABLE `subscriptions` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `UserId` char(36) COLLATE ascii_general_ci NOT NULL,
    `PlanId` char(36) COLLATE ascii_general_ci NOT NULL,
    `Status` int NOT NULL,
    `StartAt` datetime(6) NOT NULL,
    `EndAt` datetime(6) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NOT NULL,
    CONSTRAINT `PK_subscriptions` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_subscriptions_plans_PlanId` FOREIGN KEY (`PlanId`) REFERENCES `plans` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_subscriptions_users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `user_achievement_progress` (
    `UserId` char(36) COLLATE ascii_general_ci NOT NULL,
    `AchievementId` char(36) COLLATE ascii_general_ci NOT NULL,
    `MetricKey` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
    `MetricValue` int NOT NULL,
    `UpdatedAt` datetime(6) NOT NULL,
    CONSTRAINT `PK_user_achievement_progress` PRIMARY KEY (`UserId`, `AchievementId`, `MetricKey`),
    CONSTRAINT `FK_user_achievement_progress_achievements_AchievementId` FOREIGN KEY (`AchievementId`) REFERENCES `achievements` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_user_achievement_progress_users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `user_achievements` (
    `UserId` char(36) COLLATE ascii_general_ci NOT NULL,
    `AchievementId` char(36) COLLATE ascii_general_ci NOT NULL,
    `UnlockedAt` datetime(6) NOT NULL,
    CONSTRAINT `PK_user_achievements` PRIMARY KEY (`UserId`, `AchievementId`),
    CONSTRAINT `FK_user_achievements_achievements_AchievementId` FOREIGN KEY (`AchievementId`) REFERENCES `achievements` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_user_achievements_users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `user_blocks` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `UserId` char(36) COLLATE ascii_general_ci NOT NULL,
    `BlockedByAdminId` char(36) COLLATE ascii_general_ci NOT NULL,
    `BlockType` int NOT NULL,
    `ReasonCode` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
    `Description` varchar(1000) CHARACTER SET utf8mb4 NOT NULL,
    `StartsAt` datetime(6) NOT NULL,
    `EndsAt` datetime(6) NULL,
    `IsActive` tinyint(1) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    CONSTRAINT `PK_user_blocks` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_user_blocks_users_BlockedByAdminId` FOREIGN KEY (`BlockedByAdminId`) REFERENCES `users` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_user_blocks_users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `user_follow_artists` (
    `UserId` char(36) COLLATE ascii_general_ci NOT NULL,
    `ArtistId` char(36) COLLATE ascii_general_ci NOT NULL,
    `FollowedAt` datetime(6) NOT NULL,
    `NotifyNewReleases` tinyint(1) NOT NULL,
    CONSTRAINT `PK_user_follow_artists` PRIMARY KEY (`UserId`, `ArtistId`),
    CONSTRAINT `FK_user_follow_artists_artists_ArtistId` FOREIGN KEY (`ArtistId`) REFERENCES `artists` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_user_follow_artists_users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `user_profiles` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `DisplayName` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `AvatarUrl` varchar(500) CHARACTER SET utf8mb4 NULL,
    `Country` char(2) CHARACTER SET utf8mb4 NULL,
    `Bio` varchar(1000) CHARACTER SET utf8mb4 NULL,
    `UpdatedAt` datetime(6) NOT NULL,
    `DateOfBirth` date NOT NULL,
    `Gender` int NOT NULL,
    CONSTRAINT `PK_user_profiles` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_user_profiles_users_Id` FOREIGN KEY (`Id`) REFERENCES `users` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `user_roles` (
    `UserId` char(36) COLLATE ascii_general_ci NOT NULL,
    `RoleId` char(36) COLLATE ascii_general_ci NOT NULL,
    CONSTRAINT `PK_user_roles` PRIMARY KEY (`UserId`, `RoleId`),
    CONSTRAINT `FK_user_roles_roles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `roles` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_user_roles_users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `user_saved_albums` (
    `UserId` char(36) COLLATE ascii_general_ci NOT NULL,
    `AlbumId` char(36) COLLATE ascii_general_ci NOT NULL,
    `SavedAt` datetime(6) NOT NULL,
    CONSTRAINT `PK_user_saved_albums` PRIMARY KEY (`UserId`, `AlbumId`),
    CONSTRAINT `FK_user_saved_albums_albums_AlbumId` FOREIGN KEY (`AlbumId`) REFERENCES `albums` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_user_saved_albums_users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `verification_requests` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `ArtistId` char(36) COLLATE ascii_general_ci NOT NULL,
    `SubmittedByUserId` char(36) COLLATE ascii_general_ci NOT NULL,
    `Status` int NOT NULL,
    `AdminId` char(36) COLLATE ascii_general_ci NULL,
    `AdminNote` varchar(2000) CHARACTER SET utf8mb4 NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NOT NULL,
    CONSTRAINT `PK_verification_requests` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_verification_requests_artists_ArtistId` FOREIGN KEY (`ArtistId`) REFERENCES `artists` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_verification_requests_users_AdminId` FOREIGN KEY (`AdminId`) REFERENCES `users` (`Id`) ON DELETE SET NULL,
    CONSTRAINT `FK_verification_requests_users_SubmittedByUserId` FOREIGN KEY (`SubmittedByUserId`) REFERENCES `users` (`Id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;

CREATE TABLE `listening_events` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `UserId` char(36) COLLATE ascii_general_ci NULL,
    `TrackId` char(36) COLLATE ascii_general_ci NOT NULL,
    `PlayedAt` datetime(6) NOT NULL,
    `MsPlayed` int NOT NULL,
    `DeviceType` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
    `CountryCode` char(2) CHARACTER SET utf8mb4 NULL,
    `SourceType` varchar(50) CHARACTER SET utf8mb4 NULL,
    `SourceId` char(36) COLLATE ascii_general_ci NULL,
    CONSTRAINT `PK_listening_events` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_listening_events_tracks_TrackId` FOREIGN KEY (`TrackId`) REFERENCES `tracks` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_listening_events_users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE SET NULL
) CHARACTER SET=utf8mb4;

CREATE TABLE `lyrics` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `TrackId` char(36) COLLATE ascii_general_ci NOT NULL,
    `LanguageCode` varchar(10) CHARACTER SET utf8mb4 NOT NULL,
    `PlainText` longtext CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK_lyrics` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_lyrics_tracks_TrackId` FOREIGN KEY (`TrackId`) REFERENCES `tracks` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `lyrics_segments` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `TrackId` char(36) COLLATE ascii_general_ci NOT NULL,
    `StartMs` int NOT NULL,
    `EndMs` int NOT NULL,
    `Text` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK_lyrics_segments` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_lyrics_segments_tracks_TrackId` FOREIGN KEY (`TrackId`) REFERENCES `tracks` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `track_artists` (
    `TrackId` char(36) COLLATE ascii_general_ci NOT NULL,
    `ArtistId` char(36) COLLATE ascii_general_ci NOT NULL,
    `Role` int NOT NULL,
    CONSTRAINT `PK_track_artists` PRIMARY KEY (`TrackId`, `ArtistId`),
    CONSTRAINT `FK_track_artists_artists_ArtistId` FOREIGN KEY (`ArtistId`) REFERENCES `artists` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_track_artists_tracks_TrackId` FOREIGN KEY (`TrackId`) REFERENCES `tracks` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `track_genres` (
    `TrackId` char(36) COLLATE ascii_general_ci NOT NULL,
    `GenreId` char(36) COLLATE ascii_general_ci NOT NULL,
    CONSTRAINT `PK_track_genres` PRIMARY KEY (`TrackId`, `GenreId`),
    CONSTRAINT `FK_track_genres_genres_GenreId` FOREIGN KEY (`GenreId`) REFERENCES `genres` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_track_genres_tracks_TrackId` FOREIGN KEY (`TrackId`) REFERENCES `tracks` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `track_listen_heatmap` (
    `TrackId` char(36) COLLATE ascii_general_ci NOT NULL,
    `SecondIndex` int NOT NULL,
    `PlaysCount` int NOT NULL,
    `UpdatedAt` datetime(6) NOT NULL,
    CONSTRAINT `PK_track_listen_heatmap` PRIMARY KEY (`TrackId`, `SecondIndex`),
    CONSTRAINT `FK_track_listen_heatmap_tracks_TrackId` FOREIGN KEY (`TrackId`) REFERENCES `tracks` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `track_moods` (
    `TrackId` char(36) COLLATE ascii_general_ci NOT NULL,
    `MoodId` char(36) COLLATE ascii_general_ci NOT NULL,
    CONSTRAINT `PK_track_moods` PRIMARY KEY (`TrackId`, `MoodId`),
    CONSTRAINT `FK_track_moods_moods_MoodId` FOREIGN KEY (`MoodId`) REFERENCES `moods` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_track_moods_tracks_TrackId` FOREIGN KEY (`TrackId`) REFERENCES `tracks` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `user_saved_tracks` (
    `UserId` char(36) COLLATE ascii_general_ci NOT NULL,
    `TrackId` char(36) COLLATE ascii_general_ci NOT NULL,
    `SavedAt` datetime(6) NOT NULL,
    CONSTRAINT `PK_user_saved_tracks` PRIMARY KEY (`UserId`, `TrackId`),
    CONSTRAINT `FK_user_saved_tracks_tracks_TrackId` FOREIGN KEY (`TrackId`) REFERENCES `tracks` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_user_saved_tracks_users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `user_settings` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `LanguageCode` varchar(10) CHARACTER SET utf8mb4 NOT NULL,
    `ThemeMode` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
    `CustomThemeId` char(36) COLLATE ascii_general_ci NULL,
    `AudioQualityPreference` int NOT NULL,
    `CrossfadeMs` int NOT NULL,
    `PipEnabled` tinyint(1) NOT NULL,
    `UpdatedAt` datetime(6) NOT NULL,
    CONSTRAINT `PK_user_settings` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_user_settings_custom_themes_CustomThemeId` FOREIGN KEY (`CustomThemeId`) REFERENCES `custom_themes` (`Id`) ON DELETE SET NULL,
    CONSTRAINT `FK_user_settings_users_Id` FOREIGN KEY (`Id`) REFERENCES `users` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `payout_transactions` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `PayoutRequestId` char(36) COLLATE ascii_general_ci NOT NULL,
    `Amount` decimal(18,2) NOT NULL,
    `PaidAt` datetime(6) NOT NULL,
    `ProviderRef` varchar(256) CHARACTER SET utf8mb4 NULL,
    `Status` int NOT NULL,
    CONSTRAINT `PK_payout_transactions` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_payout_transactions_payout_requests_PayoutRequestId` FOREIGN KEY (`PayoutRequestId`) REFERENCES `payout_requests` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `playback_queue_items` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `SessionId` char(36) COLLATE ascii_general_ci NOT NULL,
    `TrackId` char(36) COLLATE ascii_general_ci NOT NULL,
    `QueueType` int NOT NULL,
    `Position` int NOT NULL,
    `AddedAt` datetime(6) NOT NULL,
    `AddedByUserId` char(36) COLLATE ascii_general_ci NOT NULL,
    CONSTRAINT `PK_playback_queue_items` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_playback_queue_items_playback_sessions_SessionId` FOREIGN KEY (`SessionId`) REFERENCES `playback_sessions` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_playback_queue_items_tracks_TrackId` FOREIGN KEY (`TrackId`) REFERENCES `tracks` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_playback_queue_items_users_AddedByUserId` FOREIGN KEY (`AddedByUserId`) REFERENCES `users` (`Id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;

CREATE TABLE `playlist_tracks` (
    `PlaylistId` char(36) COLLATE ascii_general_ci NOT NULL,
    `TrackId` char(36) COLLATE ascii_general_ci NOT NULL,
    `Position` int NOT NULL,
    `AddedAt` datetime(6) NOT NULL,
    `AddedByUserId` char(36) COLLATE ascii_general_ci NOT NULL,
    CONSTRAINT `PK_playlist_tracks` PRIMARY KEY (`PlaylistId`, `TrackId`),
    CONSTRAINT `FK_playlist_tracks_playlists_PlaylistId` FOREIGN KEY (`PlaylistId`) REFERENCES `playlists` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_playlist_tracks_tracks_TrackId` FOREIGN KEY (`TrackId`) REFERENCES `tracks` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `shared_room_members` (
    `RoomId` char(36) COLLATE ascii_general_ci NOT NULL,
    `UserId` char(36) COLLATE ascii_general_ci NOT NULL,
    `Role` int NOT NULL,
    `JoinedAt` datetime(6) NOT NULL,
    `LeftAt` datetime(6) NULL,
    CONSTRAINT `PK_shared_room_members` PRIMARY KEY (`RoomId`, `UserId`),
    CONSTRAINT `FK_shared_room_members_shared_rooms_RoomId` FOREIGN KEY (`RoomId`) REFERENCES `shared_rooms` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_shared_room_members_users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `shared_room_queue` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `RoomId` char(36) COLLATE ascii_general_ci NOT NULL,
    `TrackId` char(36) COLLATE ascii_general_ci NOT NULL,
    `Position` int NOT NULL,
    `AddedByUserId` char(36) COLLATE ascii_general_ci NOT NULL,
    `AddedAt` datetime(6) NOT NULL,
    CONSTRAINT `PK_shared_room_queue` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_shared_room_queue_shared_rooms_RoomId` FOREIGN KEY (`RoomId`) REFERENCES `shared_rooms` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_shared_room_queue_tracks_TrackId` FOREIGN KEY (`TrackId`) REFERENCES `tracks` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_shared_room_queue_users_AddedByUserId` FOREIGN KEY (`AddedByUserId`) REFERENCES `users` (`Id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;

CREATE TABLE `smart_link_clicks` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `SmartLinkId` char(36) COLLATE ascii_general_ci NOT NULL,
    `ClickedAt` datetime(6) NOT NULL,
    `CountryCode` char(2) CHARACTER SET utf8mb4 NULL,
    `Referrer` varchar(500) CHARACTER SET utf8mb4 NULL,
    `DeviceType` varchar(50) CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_smart_link_clicks` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_smart_link_clicks_smart_links_SmartLinkId` FOREIGN KEY (`SmartLinkId`) REFERENCES `smart_links` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE UNIQUE INDEX `IX_achievements_Code` ON `achievements` (`Code`);

CREATE INDEX `IX_achievements_IsActive` ON `achievements` (`IsActive`);

CREATE INDEX `IX_ad_impressions_AdId` ON `ad_impressions` (`AdId`);

CREATE INDEX `IX_ad_impressions_UserId` ON `ad_impressions` (`UserId`);

CREATE INDEX `IX_ads_IsActive` ON `ads` (`IsActive`);

CREATE INDEX `IX_album_artists_ArtistId` ON `album_artists` (`ArtistId`);

CREATE INDEX `IX_albums_ReleaseDate` ON `albums` (`ReleaseDate`);

CREATE UNIQUE INDEX `IX_artist_pins_ArtistId_Position` ON `artist_pins` (`ArtistId`, `Position`);

CREATE INDEX `IX_artist_pins_EntityType_EntityId` ON `artist_pins` (`EntityType`, `EntityId`);

CREATE INDEX `IX_artist_social_links_ArtistId` ON `artist_social_links` (`ArtistId`);

CREATE UNIQUE INDEX `IX_artist_team_members_ArtistId_UserId` ON `artist_team_members` (`ArtistId`, `UserId`);

CREATE INDEX `IX_artist_team_members_UserId` ON `artist_team_members` (`UserId`);

CREATE INDEX `IX_artists_Name` ON `artists` (`Name`);

CREATE INDEX `IX_complaints_CreatedByUserId` ON `complaints` (`CreatedByUserId`);

CREATE INDEX `IX_complaints_ModeratedByAdminId` ON `complaints` (`ModeratedByAdminId`);

CREATE INDEX `IX_complaints_Status` ON `complaints` (`Status`);

CREATE INDEX `IX_complaints_TargetType_TargetId` ON `complaints` (`TargetType`, `TargetId`);

CREATE INDEX `IX_copyright_claims_EntityType_EntityId` ON `copyright_claims` (`EntityType`, `EntityId`);

CREATE INDEX `IX_copyright_claims_OwnerArtistId` ON `copyright_claims` (`OwnerArtistId`);

CREATE INDEX `IX_custom_themes_UserId` ON `custom_themes` (`UserId`);

CREATE UNIQUE INDEX `IX_genres_Name` ON `genres` (`Name`);

CREATE INDEX `IX_listening_events_TrackId_PlayedAt` ON `listening_events` (`TrackId`, `PlayedAt`);

CREATE INDEX `IX_listening_events_UserId_PlayedAt` ON `listening_events` (`UserId`, `PlayedAt`);

CREATE UNIQUE INDEX `IX_lyrics_TrackId` ON `lyrics` (`TrackId`);

CREATE INDEX `IX_lyrics_segments_TrackId_StartMs` ON `lyrics_segments` (`TrackId`, `StartMs`);

CREATE INDEX `IX_notifications_CreatedAt` ON `notifications` (`CreatedAt`);

CREATE INDEX `IX_notifications_UserId_IsRead` ON `notifications` (`UserId`, `IsRead`);

CREATE INDEX `IX_payout_requests_AdminId` ON `payout_requests` (`AdminId`);

CREATE INDEX `IX_payout_requests_ArtistId` ON `payout_requests` (`ArtistId`);

CREATE INDEX `IX_payout_transactions_PayoutRequestId` ON `payout_transactions` (`PayoutRequestId`);

CREATE UNIQUE INDEX `IX_permissions_Name` ON `permissions` (`Name`);

CREATE UNIQUE INDEX `IX_plans_Name_Period` ON `plans` (`Name`, `Period`);

CREATE INDEX `IX_playback_queue_items_AddedByUserId` ON `playback_queue_items` (`AddedByUserId`);

CREATE UNIQUE INDEX `IX_playback_queue_items_SessionId_Position` ON `playback_queue_items` (`SessionId`, `Position`);

CREATE INDEX `IX_playback_queue_items_TrackId` ON `playback_queue_items` (`TrackId`);

CREATE INDEX `IX_playback_sessions_ContextType_ContextId` ON `playback_sessions` (`ContextType`, `ContextId`);

CREATE INDEX `IX_playback_sessions_StartedAt` ON `playback_sessions` (`StartedAt`);

CREATE INDEX `IX_playback_sessions_UserId` ON `playback_sessions` (`UserId`);

CREATE INDEX `IX_playlist_tracks_TrackId` ON `playlist_tracks` (`TrackId`);

CREATE INDEX `IX_playlists_IsDeleted` ON `playlists` (`IsDeleted`);

CREATE INDEX `IX_playlists_UserId` ON `playlists` (`UserId`);

CREATE UNIQUE INDEX `IX_refresh_tokens_TokenHash` ON `refresh_tokens` (`TokenHash`);

CREATE INDEX `IX_refresh_tokens_UserId` ON `refresh_tokens` (`UserId`);

CREATE INDEX `IX_release_notification_templates_ArtistId` ON `release_notification_templates` (`ArtistId`);

CREATE INDEX `IX_role_permissions_PermissionId` ON `role_permissions` (`PermissionId`);

CREATE UNIQUE INDEX `IX_roles_Name` ON `roles` (`Name`);

CREATE INDEX `IX_shared_room_members_UserId` ON `shared_room_members` (`UserId`);

CREATE INDEX `IX_shared_room_queue_AddedByUserId` ON `shared_room_queue` (`AddedByUserId`);

CREATE UNIQUE INDEX `IX_shared_room_queue_RoomId_Position` ON `shared_room_queue` (`RoomId`, `Position`);

CREATE INDEX `IX_shared_room_queue_TrackId` ON `shared_room_queue` (`TrackId`);

CREATE INDEX `IX_shared_rooms_CreatedAt` ON `shared_rooms` (`CreatedAt`);

CREATE INDEX `IX_shared_rooms_HostUserId` ON `shared_rooms` (`HostUserId`);

CREATE INDEX `IX_shared_rooms_Status` ON `shared_rooms` (`Status`);

CREATE INDEX `IX_smart_link_clicks_ClickedAt` ON `smart_link_clicks` (`ClickedAt`);

CREATE INDEX `IX_smart_link_clicks_SmartLinkId` ON `smart_link_clicks` (`SmartLinkId`);

CREATE UNIQUE INDEX `IX_smart_links_Code` ON `smart_links` (`Code`);

CREATE INDEX `IX_smart_links_CreatedByUserId` ON `smart_links` (`CreatedByUserId`);

CREATE INDEX `IX_smart_links_EntityType_EntityId` ON `smart_links` (`EntityType`, `EntityId`);

CREATE INDEX `IX_subscriptions_EndAt` ON `subscriptions` (`EndAt`);

CREATE INDEX `IX_subscriptions_PlanId` ON `subscriptions` (`PlanId`);

CREATE INDEX `IX_subscriptions_UserId` ON `subscriptions` (`UserId`);

CREATE UNIQUE INDEX `IX_themes_Name` ON `themes` (`Name`);

CREATE INDEX `IX_track_artists_ArtistId` ON `track_artists` (`ArtistId`);

CREATE INDEX `IX_track_genres_GenreId` ON `track_genres` (`GenreId`);

CREATE INDEX `IX_track_moods_MoodId` ON `track_moods` (`MoodId`);

CREATE INDEX `IX_tracks_AlbumId` ON `tracks` (`AlbumId`);

CREATE INDEX `IX_user_achievement_progress_AchievementId` ON `user_achievement_progress` (`AchievementId`);

CREATE INDEX `IX_user_achievements_AchievementId` ON `user_achievements` (`AchievementId`);

CREATE INDEX `IX_user_blocks_BlockedByAdminId` ON `user_blocks` (`BlockedByAdminId`);

CREATE INDEX `IX_user_blocks_IsActive` ON `user_blocks` (`IsActive`);

CREATE INDEX `IX_user_blocks_UserId` ON `user_blocks` (`UserId`);

CREATE INDEX `IX_user_follow_artists_ArtistId` ON `user_follow_artists` (`ArtistId`);

CREATE INDEX `IX_user_profiles_DisplayName` ON `user_profiles` (`DisplayName`);

CREATE INDEX `IX_user_roles_RoleId` ON `user_roles` (`RoleId`);

CREATE INDEX `IX_user_saved_albums_AlbumId` ON `user_saved_albums` (`AlbumId`);

CREATE INDEX `IX_user_saved_tracks_TrackId` ON `user_saved_tracks` (`TrackId`);

CREATE INDEX `IX_user_settings_CustomThemeId` ON `user_settings` (`CustomThemeId`);

CREATE UNIQUE INDEX `IX_users_Email` ON `users` (`Email`);

CREATE INDEX `IX_verification_requests_AdminId` ON `verification_requests` (`AdminId`);

CREATE INDEX `IX_verification_requests_ArtistId` ON `verification_requests` (`ArtistId`);

CREATE INDEX `IX_verification_requests_CreatedAt` ON `verification_requests` (`CreatedAt`);

CREATE INDEX `IX_verification_requests_Status` ON `verification_requests` (`Status`);

CREATE INDEX `IX_verification_requests_SubmittedByUserId` ON `verification_requests` (`SubmittedByUserId`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260306202200_InitialDDD', '9.0.0');

ALTER TABLE `users` DROP COLUMN `LoginCodeExpiryUtc`;

ALTER TABLE `users` DROP COLUMN `LoginCodeHash`;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260306215848_ÐfixLoginWithCode', '9.0.0');

ALTER TABLE `artists` MODIFY COLUMN `BannerUrl` varchar(2048) CHARACTER SET utf8mb4 NULL;

ALTER TABLE `artists` MODIFY COLUMN `AvatarUrl` varchar(2048) CHARACTER SET utf8mb4 NULL;

CREATE INDEX `IX_artists_CreatedAt` ON `artists` (`CreatedAt` DESC);

CREATE INDEX `IX_artists_VerificationStatus_CreatedAt` ON `artists` (`VerificationStatus`, `CreatedAt`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260307174125_IncreaseArtistUrlLength', '9.0.0');

ALTER TABLE `moods` MODIFY COLUMN `CoverUrl` varchar(2048) CHARACTER SET utf8mb4 NULL;

ALTER TABLE `genres` MODIFY COLUMN `CoverUrl` varchar(2048) CHARACTER SET utf8mb4 NULL;

ALTER TABLE `albums` MODIFY COLUMN `CoverUrl` varchar(2048) CHARACTER SET utf8mb4 NULL;

CREATE TABLE `outbox_messages` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `Type` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
    `Content` longtext CHARACTER SET utf8mb4 NOT NULL,
    `OccurredOnUtc` datetime(6) NOT NULL,
    `ProcessedOnUtc` datetime(6) NULL,
    `Error` varchar(2000) CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_outbox_messages` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE UNIQUE INDEX `IX_artist_team_members_ArtistId_Role` ON `artist_team_members` (`ArtistId`, `Role`);

CREATE INDEX `IX_albums_CreatedAt` ON `albums` (`CreatedAt` DESC);

CREATE INDEX `IX_albums_Title` ON `albums` (`Title`);

CREATE INDEX `IX_albums_VisibilityStatus_CreatedAt` ON `albums` (`VisibilityStatus`, `CreatedAt`);

CREATE INDEX `IX_outbox_messages_ProcessedOnUtc_OccurredOnUtc` ON `outbox_messages` (`ProcessedOnUtc`, `OccurredOnUtc`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260307202128_AddOutboxMessages', '9.0.0');

COMMIT;

