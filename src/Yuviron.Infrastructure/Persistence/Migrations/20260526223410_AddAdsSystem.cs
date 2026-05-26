using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yuviron.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAdsSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ad_impressions_users_UserId",
                table: "ad_impressions");

            migrationBuilder.RenameColumn(
                name: "MediaUrl",
                table: "ads",
                newName: "ImageUrl");

            migrationBuilder.AddColumn<string>(
                name: "AdvertiserName",
                table: "ads",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "AudioUrl",
                table: "ads",
                type: "varchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ClickUrl",
                table: "ads",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ads",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "ads",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ads",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "ads",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "ad_impressions",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldNullable: true)
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AddColumn<DateTime>(
                name: "ClickedAt",
                table: "ad_impressions",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsClicked",
                table: "ad_impressions",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_ads_IsDeleted",
                table: "ads",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_ad_impressions_ShownAt",
                table: "ad_impressions",
                column: "ShownAt");

            migrationBuilder.AddForeignKey(
                name: "FK_ad_impressions_users_UserId",
                table: "ad_impressions",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ad_impressions_users_UserId",
                table: "ad_impressions");

            migrationBuilder.DropIndex(
                name: "IX_ads_IsDeleted",
                table: "ads");

            migrationBuilder.DropIndex(
                name: "IX_ad_impressions_ShownAt",
                table: "ad_impressions");

            migrationBuilder.DropColumn(
                name: "AdvertiserName",
                table: "ads");

            migrationBuilder.DropColumn(
                name: "AudioUrl",
                table: "ads");

            migrationBuilder.DropColumn(
                name: "ClickUrl",
                table: "ads");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ads");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ads");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ads");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ads");

            migrationBuilder.DropColumn(
                name: "ClickedAt",
                table: "ad_impressions");

            migrationBuilder.DropColumn(
                name: "IsClicked",
                table: "ad_impressions");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "ads",
                newName: "MediaUrl");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "ad_impressions",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AddForeignKey(
                name: "FK_ad_impressions_users_UserId",
                table: "ad_impressions",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
