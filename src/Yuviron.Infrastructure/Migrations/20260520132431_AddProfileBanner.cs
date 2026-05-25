using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yuviron.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileBanner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UseAnimatedAvatar",
                table: "user_settings");

            migrationBuilder.AddColumn<string>(
                name: "BannerUrl",
                table: "user_profiles",
                type: "varchar(2048)",
                maxLength: 2048,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BannerUrl",
                table: "user_profiles");

            migrationBuilder.AddColumn<bool>(
                name: "UseAnimatedAvatar",
                table: "user_settings",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }
    }
}
