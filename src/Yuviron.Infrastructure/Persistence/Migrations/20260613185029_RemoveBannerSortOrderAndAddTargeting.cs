using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yuviron.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBannerSortOrderAndAddTargeting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_banners_IsActive_SortOrder",
                table: "banners");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "banners");

            migrationBuilder.AddColumn<string>(
                name: "TargetCountries",
                table: "banners",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "TargetGenres",
                table: "banners",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "DurationDays",
                table: "banner_requests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TargetCountries",
                table: "banner_requests",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "TargetGenres",
                table: "banner_requests",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_banners_IsActive",
                table: "banners",
                column: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_banners_IsActive",
                table: "banners");

            migrationBuilder.DropColumn(
                name: "TargetCountries",
                table: "banners");

            migrationBuilder.DropColumn(
                name: "TargetGenres",
                table: "banners");

            migrationBuilder.DropColumn(
                name: "DurationDays",
                table: "banner_requests");

            migrationBuilder.DropColumn(
                name: "TargetCountries",
                table: "banner_requests");

            migrationBuilder.DropColumn(
                name: "TargetGenres",
                table: "banner_requests");

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "banners",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_banners_IsActive_SortOrder",
                table: "banners",
                columns: new[] { "IsActive", "SortOrder" });
        }
    }
}
