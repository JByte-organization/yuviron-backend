using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yuviron.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddArtistIdToBanners : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ArtistId",
                table: "banners",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<DateTime>(
                name: "StartNotificationSentAtUtc",
                table: "banners",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartsAtUtc",
                table: "banners",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_banners_ArtistId",
                table: "banners",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_banners_IsActive_StartsAtUtc_StartNotificationSentAtUtc",
                table: "banners",
                columns: new[] { "IsActive", "StartsAtUtc", "StartNotificationSentAtUtc" });

            migrationBuilder.AddForeignKey(
                name: "FK_banners_artists_ArtistId",
                table: "banners",
                column: "ArtistId",
                principalTable: "artists",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_banners_artists_ArtistId",
                table: "banners");

            migrationBuilder.DropIndex(
                name: "IX_banners_ArtistId",
                table: "banners");

            migrationBuilder.DropIndex(
                name: "IX_banners_IsActive_StartsAtUtc_StartNotificationSentAtUtc",
                table: "banners");

            migrationBuilder.DropColumn(
                name: "ArtistId",
                table: "banners");

            migrationBuilder.DropColumn(
                name: "StartNotificationSentAtUtc",
                table: "banners");

            migrationBuilder.DropColumn(
                name: "StartsAtUtc",
                table: "banners");
        }
    }
}
