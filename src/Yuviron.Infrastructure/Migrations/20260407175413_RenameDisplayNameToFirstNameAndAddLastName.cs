using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yuviron.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameDisplayNameToFirstNameAndAddLastName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DisplayName",
                table: "user_profiles",
                newName: "FirstName");

            migrationBuilder.RenameIndex(
                name: "IX_user_profiles_DisplayName",
                table: "user_profiles",
                newName: "IX_user_profiles_FirstName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "user_profiles",
                newName: "DisplayName");

            migrationBuilder.RenameIndex(
                name: "IX_user_profiles_FirstName",
                table: "user_profiles",
                newName: "IX_user_profiles_DisplayName");
        }
    }
}
