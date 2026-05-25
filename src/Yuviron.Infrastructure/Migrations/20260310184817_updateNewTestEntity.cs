using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yuviron.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updateNewTestEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrackTest_tracks_TrackId",
                table: "TrackTest");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TrackTest",
                table: "TrackTest");

            migrationBuilder.DropIndex(
                name: "IX_TrackTest_TrackId",
                table: "TrackTest");

            migrationBuilder.DropColumn(
                name: "LanguageCode",
                table: "TrackTest");

            migrationBuilder.DropColumn(
                name: "PlainText",
                table: "TrackTest");

            migrationBuilder.DropColumn(
                name: "TrackId",
                table: "TrackTest");

            migrationBuilder.RenameTable(
                name: "TrackTest",
                newName: "TrackTests");

            migrationBuilder.AddColumn<string>(
                name: "Text",
                table: "TrackTests",
                type: "varchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TrackTests",
                table: "TrackTests",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TrackTests",
                table: "TrackTests");

            migrationBuilder.DropColumn(
                name: "Text",
                table: "TrackTests");

            migrationBuilder.RenameTable(
                name: "TrackTests",
                newName: "TrackTest");

            migrationBuilder.AddColumn<string>(
                name: "LanguageCode",
                table: "TrackTest",
                type: "varchar(15)",
                maxLength: 15,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PlainText",
                table: "TrackTest",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<Guid>(
                name: "TrackId",
                table: "TrackTest",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TrackTest",
                table: "TrackTest",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_TrackTest_TrackId",
                table: "TrackTest",
                column: "TrackId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TrackTest_tracks_TrackId",
                table: "TrackTest",
                column: "TrackId",
                principalTable: "tracks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
