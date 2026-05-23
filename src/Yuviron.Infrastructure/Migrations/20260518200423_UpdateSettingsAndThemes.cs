using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yuviron.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSettingsAndThemes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LanguageCode",
                table: "user_settings");

            migrationBuilder.RenameColumn(
                name: "PipEnabled",
                table: "user_settings",
                newName: "UseAnimatedAvatar");

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

            migrationBuilder.AddColumn<bool>(
                name: "ShowFollowers",
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
                name: "BackgroundImageUrl",
                table: "custom_themes",
                type: "varchar(2048)",
                maxLength: 2048,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_user_settings_ThemeId",
                table: "user_settings",
                column: "ThemeId");

            migrationBuilder.AddForeignKey(
                name: "FK_user_settings_themes_ThemeId",
                table: "user_settings",
                column: "ThemeId",
                principalTable: "themes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_user_settings_themes_ThemeId",
                table: "user_settings");

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
                name: "ShowFollowers",
                table: "user_settings");

            migrationBuilder.DropColumn(
                name: "ThemeId",
                table: "user_settings");

            migrationBuilder.DropColumn(
                name: "BackgroundImageUrl",
                table: "custom_themes");

            migrationBuilder.RenameColumn(
                name: "UseAnimatedAvatar",
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
    }
}
