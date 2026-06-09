using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yuviron.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPreferenceTheme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_themes_Name",
                table: "themes");

            migrationBuilder.DropColumn(
                name: "ShowActivity",
                table: "user_settings");

            migrationBuilder.DropColumn(
                name: "BackgroundImageUrl",
                table: "custom_themes");

            migrationBuilder.AddColumn<string>(
                name: "BackgroundColor",
                table: "themes",
                type: "varchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PrimaryColor",
                table: "themes",
                type: "varchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "SecondaryColor",
                table: "themes",
                type: "varchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "themes",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
                name: "IsPrivate",
                table: "listening_events",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "user_notification_preferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Category = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Enabled = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_notification_preferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_notification_preferences_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_themes_UserId_Name",
                table: "themes",
                columns: new[] { "UserId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_notification_preferences_UserId_Category_Code",
                table: "user_notification_preferences",
                columns: new[] { "UserId", "Category", "Code" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_themes_users_UserId",
                table: "themes",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_themes_users_UserId",
                table: "themes");

            migrationBuilder.DropTable(
                name: "user_notification_preferences");

            migrationBuilder.DropIndex(
                name: "IX_themes_UserId_Name",
                table: "themes");

            migrationBuilder.DropColumn(
                name: "BackgroundColor",
                table: "themes");

            migrationBuilder.DropColumn(
                name: "PrimaryColor",
                table: "themes");

            migrationBuilder.DropColumn(
                name: "SecondaryColor",
                table: "themes");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "themes");

            migrationBuilder.DropColumn(
                name: "IsPrivate",
                table: "listening_events");

            migrationBuilder.AddColumn<bool>(
                name: "ShowActivity",
                table: "user_settings",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "BackgroundImageUrl",
                table: "custom_themes",
                type: "varchar(2048)",
                maxLength: 2048,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_themes_Name",
                table: "themes",
                column: "Name",
                unique: true);
        }
    }
}
