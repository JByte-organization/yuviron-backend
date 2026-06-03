using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yuviron.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveArtistWalletShadowProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArtistWallets_artists_ArtistId1",
                table: "ArtistWallets");

            migrationBuilder.DropIndex(
                name: "IX_ArtistWallets_ArtistId1",
                table: "ArtistWallets");

            migrationBuilder.DropColumn(
                name: "ArtistId1",
                table: "ArtistWallets");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ArtistId1",
                table: "ArtistWallets",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_ArtistWallets_ArtistId1",
                table: "ArtistWallets",
                column: "ArtistId1",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ArtistWallets_artists_ArtistId1",
                table: "ArtistWallets",
                column: "ArtistId1",
                principalTable: "artists",
                principalColumn: "Id");
        }
    }
}
