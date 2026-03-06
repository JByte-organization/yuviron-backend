using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yuviron.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ÐfixLoginWithCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LoginCodeExpiryUtc",
                table: "users");

            migrationBuilder.DropColumn(
                name: "LoginCodeHash",
                table: "users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LoginCodeExpiryUtc",
                table: "users",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LoginCodeHash",
                table: "users",
                type: "varchar(256)",
                maxLength: 256,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
