using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yuviron.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_users_Email",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_moods_Name",
                table: "moods");

            migrationBuilder.DropIndex(
                name: "IX_genres_Name",
                table: "genres");

            migrationBuilder.CreateIndex(
                name: "IX_users_Email_IsDeleted",
                table: "users",
                columns: new[] { "Email", "IsDeleted" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_moods_Name_IsDeleted",
                table: "moods",
                columns: new[] { "Name", "IsDeleted" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_genres_Name_IsDeleted",
                table: "genres",
                columns: new[] { "Name", "IsDeleted" },
                unique: true);
        }
    }
}
