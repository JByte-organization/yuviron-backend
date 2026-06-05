using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yuviron.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialStudioContentSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropTable(
                //name: "lyrics_segments");

            //migrationBuilder.DropIndex(
            //name: "IX_user_devices_UserId_DeviceName_BrowserName",
            //table: "user_devices");

            migrationBuilder.AddColumn<string>(
                name: "Fingerprint",
                table: "user_devices",
                type: "varchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<Guid>(
                name: "ArtistId",
                table: "playlists",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateTable(
                name: "banner_requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ArtistId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    SubmittedByUserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    AlbumId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Title = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BannerUrl = table.Column<string>(type: "varchar(2048)", maxLength: 2048, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AdminNotes = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsPaid = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    StripeSessionId = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StripePaymentIntentId = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_banner_requests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_banner_requests_albums_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "albums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_banner_requests_artists_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "artists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_banner_requests_users_SubmittedByUserId",
                        column: x => x.SubmittedByUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "user_saved_playlist",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PlaylistId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    SavedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_saved_playlist", x => new { x.UserId, x.PlaylistId });
                    table.ForeignKey(
                        name: "FK_user_saved_playlist_playlists_PlaylistId",
                        column: x => x.PlaylistId,
                        principalTable: "playlists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_saved_playlist_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_user_devices_UserId_Fingerprint",
                table: "user_devices",
                columns: new[] { "UserId", "Fingerprint" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_playlists_ArtistId",
                table: "playlists",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_banner_requests_AlbumId",
                table: "banner_requests",
                column: "AlbumId");

            migrationBuilder.CreateIndex(
                name: "IX_banner_requests_ArtistId",
                table: "banner_requests",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_banner_requests_Status",
                table: "banner_requests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_banner_requests_SubmittedByUserId",
                table: "banner_requests",
                column: "SubmittedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_user_saved_playlist_PlaylistId",
                table: "user_saved_playlist",
                column: "PlaylistId");

            migrationBuilder.AddForeignKey(
                name: "FK_playlists_artists_ArtistId",
                table: "playlists",
                column: "ArtistId",
                principalTable: "artists",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_playlists_artists_ArtistId",
                table: "playlists");

            migrationBuilder.DropTable(
                name: "banner_requests");

            migrationBuilder.DropTable(
                name: "user_saved_playlist");

            migrationBuilder.DropIndex(
                name: "IX_user_devices_UserId_Fingerprint",
                table: "user_devices");

            migrationBuilder.DropIndex(
                name: "IX_playlists_ArtistId",
                table: "playlists");

            migrationBuilder.DropColumn(
                name: "Fingerprint",
                table: "user_devices");

            migrationBuilder.DropColumn(
                name: "ArtistId",
                table: "playlists");

            migrationBuilder.CreateTable(
                name: "lyrics_segments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TrackId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EndMs = table.Column<int>(type: "int", nullable: false),
                    StartMs = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lyrics_segments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_lyrics_segments_tracks_TrackId",
                        column: x => x.TrackId,
                        principalTable: "tracks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_user_devices_UserId_DeviceName_BrowserName",
                table: "user_devices",
                columns: new[] { "UserId", "DeviceName", "BrowserName" });

            migrationBuilder.CreateIndex(
                name: "IX_lyrics_segments_TrackId_StartMs",
                table: "lyrics_segments",
                columns: new[] { "TrackId", "StartMs" });
        }
    }
}
