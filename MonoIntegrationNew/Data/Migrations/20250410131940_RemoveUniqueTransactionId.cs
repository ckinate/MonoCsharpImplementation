using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MonoIntegrationNew.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUniqueTransactionId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MonoTransactions_TransactionId",
                table: "MonoTransactions");

            migrationBuilder.AlterColumn<string>(
                name: "TransactionId",
                table: "MonoTransactions",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TransactionId",
                table: "MonoTransactions",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MonoTransactions_TransactionId",
                table: "MonoTransactions",
                column: "TransactionId",
                unique: true,
                filter: "[TransactionId] IS NOT NULL");
        }
    }
}
