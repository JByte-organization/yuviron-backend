using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yuviron.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixFileMetadataNaming : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FileMetadatas_users_UserId",
                table: "FileMetadatas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FileMetadatas",
                table: "FileMetadatas");

            migrationBuilder.RenameTable(
                name: "FileMetadatas",
                newName: "file_metadata");

            migrationBuilder.RenameIndex(
                name: "IX_FileMetadatas_UserId",
                table: "file_metadata",
                newName: "IX_file_metadata_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_FileMetadatas_IsTemporary",
                table: "file_metadata",
                newName: "IX_file_metadata_IsTemporary");

            migrationBuilder.RenameIndex(
                name: "IX_FileMetadatas_CreatedAt",
                table: "file_metadata",
                newName: "IX_file_metadata_CreatedAt");

            migrationBuilder.AddPrimaryKey(
                name: "PK_file_metadata",
                table: "file_metadata",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_file_metadata_users_UserId",
                table: "file_metadata",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_file_metadata_users_UserId",
                table: "file_metadata");

            migrationBuilder.DropPrimaryKey(
                name: "PK_file_metadata",
                table: "file_metadata");

            migrationBuilder.RenameTable(
                name: "file_metadata",
                newName: "FileMetadatas");

            migrationBuilder.RenameIndex(
                name: "IX_file_metadata_UserId",
                table: "FileMetadatas",
                newName: "IX_FileMetadatas_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_file_metadata_IsTemporary",
                table: "FileMetadatas",
                newName: "IX_FileMetadatas_IsTemporary");

            migrationBuilder.RenameIndex(
                name: "IX_file_metadata_CreatedAt",
                table: "FileMetadatas",
                newName: "IX_FileMetadatas_CreatedAt");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FileMetadatas",
                table: "FileMetadatas",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FileMetadatas_users_UserId",
                table: "FileMetadatas",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
