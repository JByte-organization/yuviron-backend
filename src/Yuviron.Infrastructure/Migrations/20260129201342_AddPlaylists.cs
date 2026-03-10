using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yuviron.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPlaylists : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_playlists_users_OwnerUserId",
                table: "playlists");

            migrationBuilder.RenameColumn(
                name: "OwnerUserId",
                table: "playlists",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_playlists_OwnerUserId",
                table: "playlists",
                newName: "IX_playlists_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_playlists_users_UserId",
                table: "playlists",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_playlists_users_UserId",
                table: "playlists");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "playlists",
                newName: "OwnerUserId");

            migrationBuilder.RenameIndex(
                name: "IX_playlists_UserId",
                table: "playlists",
                newName: "IX_playlists_OwnerUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_playlists_users_OwnerUserId",
                table: "playlists",
                column: "OwnerUserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
