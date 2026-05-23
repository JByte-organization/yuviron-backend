using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yuviron.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBannerUrlFromUserProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_user_settings_themes_ThemeId",
                table: "user_settings");

            migrationBuilder.DropTable(
                name: "user_follow_user");

            migrationBuilder.DropIndex(
                name: "IX_user_settings_ThemeId",
                table: "user_settings");

            migrationBuilder.DropColumn(
                name: "MakePlaylistsPublicByDefault",
                table: "user_settings");

            migrationBuilder.DropColumn(
                name: "PrivateSession",
                table: "user_settings");

            migrationBuilder.DropColumn(
                name: "ShowActivity",
                table: "user_settings");

            migrationBuilder.DropColumn(
                name: "ThemeId",
                table: "user_settings");

            migrationBuilder.DropColumn(
                name: "BannerUrl",
                table: "user_profiles");

            migrationBuilder.DropColumn(
                name: "BackgroundImageUrl",
                table: "custom_themes");

            migrationBuilder.RenameColumn(
                name: "ShowFollowers",
                table: "user_settings",
                newName: "PipEnabled");

            migrationBuilder.AddColumn<string>(
                name: "LanguageCode",
                table: "user_settings",
                type: "varchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LanguageCode",
                table: "user_settings");

            migrationBuilder.RenameColumn(
                name: "PipEnabled",
                table: "user_settings",
                newName: "ShowFollowers");

            migrationBuilder.AddColumn<bool>(
                name: "MakePlaylistsPublicByDefault",
                table: "user_settings",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PrivateSession",
                table: "user_settings",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowActivity",
                table: "user_settings",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "ThemeId",
                table: "user_settings",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<string>(
                name: "BannerUrl",
                table: "user_profiles",
                type: "varchar(2048)",
                maxLength: 2048,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "BackgroundImageUrl",
                table: "custom_themes",
                type: "varchar(2048)",
                maxLength: 2048,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "user_follow_user",
                columns: table => new
                {
                    FollowerId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    FolloweeId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    FollowedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_follow_user", x => new { x.FollowerId, x.FolloweeId });
                    table.ForeignKey(
                        name: "FK_user_follow_user_users_FolloweeId",
                        column: x => x.FolloweeId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_user_follow_user_users_FollowerId",
                        column: x => x.FollowerId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_user_settings_ThemeId",
                table: "user_settings",
                column: "ThemeId");

            migrationBuilder.CreateIndex(
                name: "IX_user_follow_user_FolloweeId",
                table: "user_follow_user",
                column: "FolloweeId");

            migrationBuilder.AddForeignKey(
                name: "FK_user_settings_themes_ThemeId",
                table: "user_settings",
                column: "ThemeId",
                principalTable: "themes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
