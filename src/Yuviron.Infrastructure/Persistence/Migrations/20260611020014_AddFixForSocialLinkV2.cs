using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yuviron.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFixForSocialLinkV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_artist_social_links_ArtistId_Type",
                table: "artist_social_links");

            migrationBuilder.CreateIndex(
                name: "IX_artist_social_links_ArtistId",
                table: "artist_social_links",
                column: "ArtistId");
        }
    }
}
