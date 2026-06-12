using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yuviron.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixTableNamingConsistency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArtistWallets_artists_ArtistId",
                table: "ArtistWallets");

            migrationBuilder.DropForeignKey(
                name: "FK_WalletTransactions_ArtistWallets_WalletId",
                table: "WalletTransactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WalletTransactions",
                table: "WalletTransactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ExternalMappings",
                table: "ExternalMappings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ArtistWallets",
                table: "ArtistWallets");

            migrationBuilder.RenameTable(
                name: "WalletTransactions",
                newName: "wallet_transactions");

            migrationBuilder.RenameTable(
                name: "ExternalMappings",
                newName: "external_mappings");

            migrationBuilder.RenameTable(
                name: "ArtistWallets",
                newName: "artist_wallets");

            migrationBuilder.RenameIndex(
                name: "IX_WalletTransactions_WalletId_CreatedAt",
                table: "wallet_transactions",
                newName: "IX_wallet_transactions_WalletId_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_ExternalMappings_Provider_ExternalId_EntityType",
                table: "external_mappings",
                newName: "IX_external_mappings_Provider_ExternalId_EntityType");

            migrationBuilder.RenameIndex(
                name: "IX_ArtistWallets_ArtistId",
                table: "artist_wallets",
                newName: "IX_artist_wallets_ArtistId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_wallet_transactions",
                table: "wallet_transactions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_external_mappings",
                table: "external_mappings",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_artist_wallets",
                table: "artist_wallets",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_artist_wallets_artists_ArtistId",
                table: "artist_wallets",
                column: "ArtistId",
                principalTable: "artists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_wallet_transactions_artist_wallets_WalletId",
                table: "wallet_transactions",
                column: "WalletId",
                principalTable: "artist_wallets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_artist_wallets_artists_ArtistId",
                table: "artist_wallets");

            migrationBuilder.DropForeignKey(
                name: "FK_wallet_transactions_artist_wallets_WalletId",
                table: "wallet_transactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_wallet_transactions",
                table: "wallet_transactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_external_mappings",
                table: "external_mappings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_artist_wallets",
                table: "artist_wallets");

            migrationBuilder.RenameTable(
                name: "wallet_transactions",
                newName: "WalletTransactions");

            migrationBuilder.RenameTable(
                name: "external_mappings",
                newName: "ExternalMappings");

            migrationBuilder.RenameTable(
                name: "artist_wallets",
                newName: "ArtistWallets");

            migrationBuilder.RenameIndex(
                name: "IX_wallet_transactions_WalletId_CreatedAt",
                table: "WalletTransactions",
                newName: "IX_WalletTransactions_WalletId_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_external_mappings_Provider_ExternalId_EntityType",
                table: "ExternalMappings",
                newName: "IX_ExternalMappings_Provider_ExternalId_EntityType");

            migrationBuilder.RenameIndex(
                name: "IX_artist_wallets_ArtistId",
                table: "ArtistWallets",
                newName: "IX_ArtistWallets_ArtistId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WalletTransactions",
                table: "WalletTransactions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ExternalMappings",
                table: "ExternalMappings",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ArtistWallets",
                table: "ArtistWallets",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ArtistWallets_artists_ArtistId",
                table: "ArtistWallets",
                column: "ArtistId",
                principalTable: "artists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WalletTransactions_ArtistWallets_WalletId",
                table: "WalletTransactions",
                column: "WalletId",
                principalTable: "ArtistWallets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
