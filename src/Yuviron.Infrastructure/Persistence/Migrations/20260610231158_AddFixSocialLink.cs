using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yuviron.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFixSocialLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Временно удаляем внешний ключ
            migrationBuilder.DropForeignKey(
                name: "FK_artist_social_links_artists_ArtistId",
                table: "artist_social_links");

            // 2. Теперь MySQL разрешит удалить старый индекс
            migrationBuilder.DropIndex(
                name: "IX_artist_social_links_ArtistId",
                table: "artist_social_links");

            // 3. Создаем твой новый композитный индекс
            migrationBuilder.CreateIndex(
                name: "IX_artist_social_links_ArtistId_Type",
                table: "artist_social_links",
                columns: new[] { "ArtistId", "Type" },
                unique: true);

            // 4. Возвращаем внешний ключ (теперь он будет использовать новый индекс)
            migrationBuilder.AddForeignKey(
                name: "FK_artist_social_links_artists_ArtistId",
                table: "artist_social_links",
                column: "ArtistId",
                principalTable: "artists", // Имя главной таблицы
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade); 
                // Примечание: Если у тебя в конфигурации настроено другое поведение при удалении 
                // (например, Restrict или SetNull), замени Cascade на него.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            
            migrationBuilder.DropForeignKey(
                name: "FK_artist_social_links_artists_ArtistId",
                table: "artist_social_links");

            migrationBuilder.DropIndex(
                name: "IX_artist_social_links_ArtistId_Type",
                table: "artist_social_links");

            migrationBuilder.CreateIndex(
                name: "IX_artist_social_links_ArtistId",
                table: "artist_social_links",
                column: "ArtistId");

            migrationBuilder.AddForeignKey(
                name: "FK_artist_social_links_artists_ArtistId",
                table: "artist_social_links",
                column: "ArtistId",
                principalTable: "artists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}