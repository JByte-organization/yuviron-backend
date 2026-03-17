using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yuviron.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeTrackAlbumIdRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tracks_albums_AlbumId",
                table: "tracks");

            migrationBuilder.AlterColumn<Guid>(
                name: "AlbumId",
                table: "tracks",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldNullable: true)
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AddColumn<int>(
                name: "AlbumPosition",
                table: "tracks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_tracks_albums_AlbumId",
                table: "tracks",
                column: "AlbumId",
                principalTable: "albums",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tracks_albums_AlbumId",
                table: "tracks");

            migrationBuilder.DropColumn(
                name: "AlbumPosition",
                table: "tracks");

            migrationBuilder.AlterColumn<Guid>(
                name: "AlbumId",
                table: "tracks",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AddForeignKey(
                name: "FK_tracks_albums_AlbumId",
                table: "tracks",
                column: "AlbumId",
                principalTable: "albums",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
