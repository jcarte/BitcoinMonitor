using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace BitcoinWalletWatcher.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WalletBalances",
                columns: table => new
                {
                    BalanceId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BalanceBTC = table.Column<decimal>(nullable: false),
                    ScrapedAt = table.Column<DateTime>(nullable: false),
                    WalletId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletBalances", x => x.BalanceId);
                });

            migrationBuilder.CreateTable(
                name: "Wallets",
                columns: table => new
                {
                    WalletId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Address = table.Column<string>(nullable: true),
                    CurrentBalanceBTC = table.Column<decimal>(nullable: false),
                    LastBalanceScrapedAt = table.Column<DateTime>(nullable: false),
                    MaxBalanceBTC = table.Column<decimal>(nullable: false),
                    Notes = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wallets", x => x.WalletId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WalletBalances");

            migrationBuilder.DropTable(
                name: "Wallets");
        }
    }
}
