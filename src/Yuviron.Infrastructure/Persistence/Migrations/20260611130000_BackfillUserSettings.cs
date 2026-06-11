using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yuviron.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class BackfillUserSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                INSERT INTO user_settings (
                    Id,
                    ThemeMode,
                    AudioQualityPreference,
                    CrossfadeMs,
                    MakePlaylistsPublicByDefault,
                    ShowFollowers,
                    PrivateSession,
                    ThemeId,
                    CustomThemeId,
                    UpdatedAt
                )
                SELECT
                    u.Id,
                    'System',
                    128,
                    0,
                    1,
                    1,
                    0,
                    NULL,
                    NULL,
                    UTC_TIMESTAMP(6)
                FROM users u
                WHERE NOT EXISTS (
                    SELECT 1 FROM user_settings s WHERE s.Id = u.Id
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Backfill migrations are not reversible without knowing which rows were inserted.
        }
    }
}
