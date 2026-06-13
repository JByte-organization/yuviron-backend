using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yuviron.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBannerExpiration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EndsAtUtc",
                table: "banners",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndsAtUtc",
                table: "banner_requests",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_banners_IsActive_EndsAtUtc",
                table: "banners",
                columns: new[] { "IsActive", "EndsAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_banners_IsActive_EndsAtUtc",
                table: "banners");

            migrationBuilder.DropColumn(
                name: "EndsAtUtc",
                table: "banners");

            migrationBuilder.DropColumn(
                name: "EndsAtUtc",
                table: "banner_requests");
        }
    }
}
