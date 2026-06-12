using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yuviron.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixMissingUserFollowTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE TABLE IF NOT EXISTS `user_follow_user` (" +
                "`FollowerId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL," +
                "`FolloweeId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL," +
                "`FollowedAt` datetime(6) NOT NULL," +
                "PRIMARY KEY (`FollowerId`, `FolloweeId`)," +
                "KEY `IX_user_follow_user_FolloweeId` (`FolloweeId`)," +
                "CONSTRAINT `FK_user_follow_user_users_FolloweeId_Repair` FOREIGN KEY (`FolloweeId`) REFERENCES `users` (`Id`) ON DELETE RESTRICT," +
                "CONSTRAINT `FK_user_follow_user_users_FollowerId_Repair` FOREIGN KEY (`FollowerId`) REFERENCES `users` (`Id`) ON DELETE RESTRICT" +
            ") ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_follow_user");
        }
    }
}
