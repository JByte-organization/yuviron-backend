using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yuviron.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHlsStreamingToTrack : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HlsPlaylistUrl",
                table: "tracks",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "ProcessingStatus",
                table: "tracks",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HlsPlaylistUrl",
                table: "tracks");

            migrationBuilder.DropColumn(
                name: "ProcessingStatus",
                table: "tracks");
        }
    }
}
