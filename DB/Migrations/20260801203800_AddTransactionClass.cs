using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DB.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionClass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TransactionId",
                table: "Trains",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TransactionId",
                table: "Stations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TransactionId",
                table: "Routes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    UnitsGet = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Trains_TransactionId",
                table: "Trains",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Stations_TransactionId",
                table: "Stations",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Routes_TransactionId",
                table: "Routes",
                column: "TransactionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Routes_Transactions_TransactionId",
                table: "Routes",
                column: "TransactionId",
                principalTable: "Transactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Stations_Transactions_TransactionId",
                table: "Stations",
                column: "TransactionId",
                principalTable: "Transactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Trains_Transactions_TransactionId",
                table: "Trains",
                column: "TransactionId",
                principalTable: "Transactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Routes_Transactions_TransactionId",
                table: "Routes");

            migrationBuilder.DropForeignKey(
                name: "FK_Stations_Transactions_TransactionId",
                table: "Stations");

            migrationBuilder.DropForeignKey(
                name: "FK_Trains_Transactions_TransactionId",
                table: "Trains");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Trains_TransactionId",
                table: "Trains");

            migrationBuilder.DropIndex(
                name: "IX_Stations_TransactionId",
                table: "Stations");

            migrationBuilder.DropIndex(
                name: "IX_Routes_TransactionId",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "Trains");

            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "Stations");

            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "Routes");
        }
    }
}
