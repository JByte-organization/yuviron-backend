using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yuviron.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayCountToTracks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "artists");

            migrationBuilder.AddColumn<int>(
                name: "PlayCount",
                table: "tracks",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlayCount",
                table: "tracks");

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "artists",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }
    }
}
