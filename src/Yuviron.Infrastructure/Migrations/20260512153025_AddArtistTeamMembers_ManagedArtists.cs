using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yuviron.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddArtistTeamMembers_ManagedArtists : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                table: "artist_team_members",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_artist_team_members_UserId1",
                table: "artist_team_members",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_artist_team_members_users_UserId1",
                table: "artist_team_members",
                column: "UserId1",
                principalTable: "users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_artist_team_members_users_UserId1",
                table: "artist_team_members");

            migrationBuilder.DropIndex(
                name: "IX_artist_team_members_UserId1",
                table: "artist_team_members");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "artist_team_members");
        }
    }
}
