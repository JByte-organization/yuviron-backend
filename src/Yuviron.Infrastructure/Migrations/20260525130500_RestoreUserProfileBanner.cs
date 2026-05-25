using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yuviron.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RestoreUserProfileBanner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                SET @column_exists := (
                    SELECT COUNT(*)
                    FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE()
                      AND TABLE_NAME = 'user_profiles'
                      AND COLUMN_NAME = 'BannerUrl'
                );

                SET @sql := IF(
                    @column_exists = 0,
                    'ALTER TABLE `user_profiles` ADD COLUMN `BannerUrl` varchar(2048) CHARACTER SET utf8mb4 NULL',
                    'SELECT 1'
                );

                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                SET @column_exists := (
                    SELECT COUNT(*)
                    FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE()
                      AND TABLE_NAME = 'user_profiles'
                      AND COLUMN_NAME = 'BannerUrl'
                );

                SET @sql := IF(
                    @column_exists > 0,
                    'ALTER TABLE `user_profiles` DROP COLUMN `BannerUrl`',
                    'SELECT 1'
                );

                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
                """);
        }
    }
}
