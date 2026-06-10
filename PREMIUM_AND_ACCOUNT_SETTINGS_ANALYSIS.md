# Yuviron Settings and Theme Documentation

This document describes the current client settings flow, the theme model, and the admin CRUD for themes.

## 1. Quick settings

Quick settings are user-facing preferences that affect playback and the current client session. They are exposed through `SettingsController`.

Endpoints:

- `GET /api/me/settings/preferences`
- `PUT /api/me/settings/theme`
- `PUT /api/me/settings/audio/quality`
- `PUT /api/me/settings/audio/crossfade`
- `PUT /api/me/settings/privacy`
- `PUT /api/me/settings/private-session`

Stored data:

- `ThemeMode`
- `ThemeId`
- `CustomThemeId`
- `AudioQualityPreference`
- `CrossfadeMs`
- `MakePlaylistsPublicByDefault`
- `ShowFollowers`
- `PrivateSession`

Notes:

- `ThemeMode` is the base UI mode: `System`, `Dark`, or `White`.
- In the current policy, `ThemeMode` is available to all users.
- `PrivateSession` is premium-gated.

## 2. Deep account settings

Deep settings change account data, security, and broader user preferences. They are split across separate controllers.

### Account

`AccountController`:

- `PUT /api/me/account/profile`
- `PUT /api/me/account/details`
- `PUT /api/me/account/marketing`
- `DELETE /api/me/account`

### Security

`SecurityController`:

- `GET /api/me/security/devices`
- `DELETE /api/me/security/devices/{id}`
- `POST /api/me/security/logout-everywhere`
- `POST /api/me/security/change-password`

### Notifications

`NotificationsController`:

- `GET /api/notifications/preferences`
- `PUT /api/notifications/preferences`

What this returns:

- A grouped catalog of notification settings, not just a flat list.
- Each group contains a `Category`, `Title`, and a list of items.
- Each item contains `Code`, `Title`, `Enabled`, `DefaultEnabled`, and `IsCategoryDefault`.

Editable shape:

- `Category` identifies the group to update.
- `Code` identifies the specific preference.
- `Enabled` is the desired state.

Categories currently available:

- `System`
- `Music`
- `Social`
- `Billing`

Examples of codes:

- `System`: `all`, `new_device_login`, `new_password_changed`, `payout_settings_changed`, `team_joined`
- `Music`: `all`, `new_release`, `editorial_playlist`, `track_trending`, `track_processed`
- `Social`: `all`, `new_follower`, `artist_followers_milestone`, `playlist_favorited`
- `Billing`: `all`, `subscription_activated`, `subscription_payment_failed`, `payout_approved`, `payout_rejected`

## 3. Theme model on the client

There are two different theme concepts:

- `ThemeMode` is the global base mode for the app shell.
- `Theme` records are preset palettes with `PrimaryColor`, `SecondaryColor`, and `BackgroundColor`.
- `CustomTheme` is a separate personal palette stored per user.

Current behavior:

- Free users can use `ThemeMode` and can use free preset themes that are assigned to them or system-wide.
- Premium users can use premium-only preset themes, create their own presets, and use custom theme editing.
- A preset theme can be selected through `ThemeId`.
- A custom palette is selected through `CustomThemeId`.

### Client appearance endpoints

`AppearanceController`:

- `GET /api/me/appearance/modes`
- `PUT /api/me/appearance/mode`
- `GET /api/me/appearance/themes`
- `POST /api/me/appearance/themes`
- `PUT /api/me/appearance/themes/{id}`
- `DELETE /api/me/appearance/themes/{id}`
- `PUT /api/me/appearance/themes/{id}/activate`
- `GET /api/me/appearance/custom-theme`
- `PUT /api/me/appearance/custom-theme`

How the frontend should treat them:

- `GET /api/me/appearance/modes` returns the base app mode options only: `System`, `Dark`, `White`.
- `PUT /api/me/appearance/mode` stores only the selected base mode.
- `GET /api/me/appearance/themes` returns the theme catalog visible to the current user.
- Items in the catalog can be:
  - system themes,
  - user-owned themes,
  - premium-only themes.
- Each theme item contains:
  - `Id`
  - `Name`
  - `PrimaryColor`
  - `SecondaryColor`
  - `BackgroundColor`
  - `IsSystem`
  - `IsPremiumOnly`
  - `IsOwnedByCurrentUser`
  - `IsSelected`
- `PUT /api/me/appearance/themes/{id}` edits the current user's own preset theme.
- `DELETE /api/me/appearance/themes/{id}` deletes the current user's own preset theme.
- `PUT /api/me/appearance/themes/{id}/activate` activates a catalog theme as the current `ThemeId`.
- `GET /api/me/appearance/custom-theme` returns the personal palette attached to `CustomThemeId`.
- `PUT /api/me/appearance/custom-theme` creates or updates the personal palette and keeps it linked to `CustomThemeId`.

### Client theme rules

- `GET /api/me/appearance/themes` returns accessible themes for the current user, including free system themes and user-owned themes.
- Premium-only themes are still present in the catalog for premium users only.
- `PUT /api/me/appearance/themes/{id}/activate` allows activation of any accessible theme.
- `POST /api/me/appearance/themes` is premium-only.
- `PUT /api/me/appearance/themes/{id}` and `DELETE /api/me/appearance/themes/{id}` are only for the owner's presets.
- `PUT /api/me/appearance/custom-theme` is premium-only and manages the personal palette, not the catalog preset list.

## 4. Admin theme CRUD

The admin theme module now has a full CRUD surface and follows the same file naming pattern as the rest of the admin features.

Controller:

- `GET /api/admin/themes`
- `GET /api/admin/themes/{id}`
- `POST /api/admin/themes`
- `PUT /api/admin/themes/{id}`
- `DELETE /api/admin/themes/{id}`

Security:

- Requests implement `ISecuredRequest`
- Required permission: `AppPermission.ManageCatalog`

Admin create/update fields:

- `Name`
- `PrimaryColor`
- `SecondaryColor`
- `BackgroundColor`
- `IsPremiumOnly`
- `UserId`

Behavior:

- `UserId = null` means a system theme.
- `UserId != null` means a user-owned preset.
- `IsPremiumOnly = true` marks the preset as premium-only.
- Admin detail/list endpoints now return `IsSystem`, `IsPremiumOnly`, and `UserId`.

## 5. What is implemented now

- Admin CRUD for themes exists and is standardized.
- Admin can create presets for a specific user or as a system theme.
- Admin can change the owner of a preset during update.
- Client free users are no longer blocked from seeing free preset themes.
- Premium-only filtering is enforced where it matters: theme catalog listing and activation.
- The solution builds successfully after the changes.
