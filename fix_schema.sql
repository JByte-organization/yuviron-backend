START TRANSACTION;
CREATE TABLE IF NOT EXISTS `user_follow_user` (`FollowerId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,`FolloweeId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,`FollowedAt` datetime(6) NOT NULL,PRIMARY KEY (`FollowerId`, `FolloweeId`),KEY `IX_user_follow_user_FolloweeId` (`FolloweeId`),CONSTRAINT `FK_user_follow_user_users_FolloweeId_Repair` FOREIGN KEY (`FolloweeId`) REFERENCES `users` (`Id`) ON DELETE RESTRICT,CONSTRAINT `FK_user_follow_user_users_FollowerId_Repair` FOREIGN KEY (`FollowerId`) REFERENCES `users` (`Id`) ON DELETE RESTRICT) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260612192425_FixMissingUserFollowTable', '9.0.0');

COMMIT;

