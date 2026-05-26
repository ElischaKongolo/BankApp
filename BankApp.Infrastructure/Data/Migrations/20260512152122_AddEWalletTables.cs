using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BankApp.Infrastructure.Data.Migrations
{
    public partial class AddEWalletTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EWallets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WalletName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    WalletNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Balance = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    AvailableBalance = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    WalletType = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastTransactionDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    LinkedAccountId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EWallets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EWallets_Accounts_LinkedAccountId",
                        column: x => x.LinkedAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EWallets_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EWalletTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    BalanceBefore = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    BalanceAfter = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EWalletId = table.Column<int>(type: "INTEGER", nullable: false),
                    ToWalletId = table.Column<int>(type: "INTEGER", nullable: true),
                    FromWalletId = table.Column<int>(type: "INTEGER", nullable: true),
                    BankTransactionId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EWalletTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EWalletTransactions_EWallets_EWalletId",
                        column: x => x.EWalletId,
                        principalTable: "EWallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EWalletTransactions_EWallets_FromWalletId",
                        column: x => x.FromWalletId,
                        principalTable: "EWallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EWalletTransactions_EWallets_ToWalletId",
                        column: x => x.ToWalletId,
                        principalTable: "EWallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EWalletTransactions_Transactions_BankTransactionId",
                        column: x => x.BankTransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EWallets_LinkedAccountId",
                table: "EWallets",
                column: "LinkedAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_EWallets_UserId",
                table: "EWallets",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_EWallets_WalletNumber",
                table: "EWallets",
                column: "WalletNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EWalletTransactions_BankTransactionId",
                table: "EWalletTransactions",
                column: "BankTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_EWalletTransactions_EWalletId",
                table: "EWalletTransactions",
                column: "EWalletId");

            migrationBuilder.CreateIndex(
                name: "IX_EWalletTransactions_FromWalletId",
                table: "EWalletTransactions",
                column: "FromWalletId");

            migrationBuilder.CreateIndex(
                name: "IX_EWalletTransactions_ToWalletId",
                table: "EWalletTransactions",
                column: "ToWalletId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EWalletTransactions");

            migrationBuilder.DropTable(
                name: "EWallets");
        }
    }
}
