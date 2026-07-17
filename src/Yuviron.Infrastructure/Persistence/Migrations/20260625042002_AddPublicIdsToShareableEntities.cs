using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yuviron.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPublicIdsToShareableEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PublicId",
                table: "tracks",
                type: "varchar(32)",
                maxLength: 32,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PublicId",
                table: "playlists",
                type: "varchar(32)",
                maxLength: 32,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PublicId",
                table: "artists",
                type: "varchar(32)",
                maxLength: 32,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PublicId",
                table: "albums",
                type: "varchar(32)",
                maxLength: 32,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.Sql("UPDATE tracks SET PublicId = LOWER(SUBSTRING(REPLACE(UUID(), '-', ''), 1, 22)) WHERE PublicId IS NULL OR PublicId = '';");
            migrationBuilder.Sql("UPDATE playlists SET PublicId = LOWER(SUBSTRING(REPLACE(UUID(), '-', ''), 1, 22)) WHERE PublicId IS NULL OR PublicId = '';");
            migrationBuilder.Sql("UPDATE artists SET PublicId = LOWER(SUBSTRING(REPLACE(UUID(), '-', ''), 1, 22)) WHERE PublicId IS NULL OR PublicId = '';");
            migrationBuilder.Sql("UPDATE albums SET PublicId = LOWER(SUBSTRING(REPLACE(UUID(), '-', ''), 1, 22)) WHERE PublicId IS NULL OR PublicId = '';");

            migrationBuilder.AlterColumn<string>(
                name: "PublicId",
                table: "tracks",
                type: "varchar(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(32)",
                oldMaxLength: 32,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "PublicId",
                table: "playlists",
                type: "varchar(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(32)",
                oldMaxLength: 32,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "PublicId",
                table: "artists",
                type: "varchar(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(32)",
                oldMaxLength: 32,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "PublicId",
                table: "albums",
                type: "varchar(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(32)",
                oldMaxLength: 32,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_tracks_PublicId",
                table: "tracks",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_playlists_PublicId",
                table: "playlists",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_artists_PublicId",
                table: "artists",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_albums_PublicId",
                table: "albums",
                column: "PublicId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_tracks_PublicId",
                table: "tracks");

            migrationBuilder.DropIndex(
                name: "IX_playlists_PublicId",
                table: "playlists");

            migrationBuilder.DropIndex(
                name: "IX_artists_PublicId",
                table: "artists");

            migrationBuilder.DropIndex(
                name: "IX_albums_PublicId",
                table: "albums");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "tracks");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "playlists");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "artists");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "albums");
        }
    }
}
