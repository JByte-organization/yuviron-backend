using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yuviron.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAlbumReleaseTypeAndArtistMonthlyListeners : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MonthlyListenersCount",
                table: "artists",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ReleaseType",
                table: "albums",
                type: "int",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE albums
                SET ReleaseType = CASE
                    WHEN LOWER(TRIM(Title)) = 'singles' THEN 1
                    ELSE 3
                END
                """);

            migrationBuilder.AlterColumn<int>(
                name: "ReleaseType",
                table: "albums",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MonthlyListenersCount",
                table: "artists");

            migrationBuilder.DropColumn(
                name: "ReleaseType",
                table: "albums");
        }
    }
}
