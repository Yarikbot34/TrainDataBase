using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DB.Migrations
{
    /// <inheritdoc />
    public partial class DeleteTransactionIdFromStation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Stations_Transactions_TransactionId",
                table: "Stations");

            migrationBuilder.DropIndex(
                name: "IX_Stations_TransactionId",
                table: "Stations");

            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "Stations");

            migrationBuilder.AddColumn<bool>(
                name: "InUse",
                table: "Stations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsOnMap",
                table: "Stations",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InUse",
                table: "Stations");

            migrationBuilder.DropColumn(
                name: "IsOnMap",
                table: "Stations");

            migrationBuilder.AddColumn<int>(
                name: "TransactionId",
                table: "Stations",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stations_TransactionId",
                table: "Stations",
                column: "TransactionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Stations_Transactions_TransactionId",
                table: "Stations",
                column: "TransactionId",
                principalTable: "Transactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
