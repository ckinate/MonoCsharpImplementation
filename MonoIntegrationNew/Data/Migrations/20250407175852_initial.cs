using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MonoIntegrationNew.Data.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MonoLinkingRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomerEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    MonoCustomerId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MonoAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MonoUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonoLinkingRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MonoAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MonoAccountId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    AccountName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccountNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BankName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BankCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccountType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DataStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LinkingRequestId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonoAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MonoAccounts_MonoLinkingRequests_LinkingRequestId",
                        column: x => x.LinkingRequestId,
                        principalTable: "MonoLinkingRequests",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MonoTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MonoAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransactionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Narration = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AccountId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonoTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MonoTransactions_MonoAccounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "MonoAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MonoAccounts_LinkingRequestId",
                table: "MonoAccounts",
                column: "LinkingRequestId",
                unique: true,
                filter: "[LinkingRequestId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MonoAccounts_MonoAccountId",
                table: "MonoAccounts",
                column: "MonoAccountId",
                unique: true,
                filter: "[MonoAccountId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MonoLinkingRequests_Reference",
                table: "MonoLinkingRequests",
                column: "Reference",
                unique: true,
                filter: "[Reference] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MonoTransactions_AccountId",
                table: "MonoTransactions",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_MonoTransactions_TransactionId",
                table: "MonoTransactions",
                column: "TransactionId",
                unique: true,
                filter: "[TransactionId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MonoTransactions");

            migrationBuilder.DropTable(
                name: "MonoAccounts");

            migrationBuilder.DropTable(
                name: "MonoLinkingRequests");
        }
    }
}
