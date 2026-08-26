using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DB.Migrations
{
    /// <inheritdoc />
    public partial class WriteRowNumberInTrainAndRow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RowInFile",
                table: "Trains",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RowInFile",
                table: "Routes",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowInFile",
                table: "Trains");

            migrationBuilder.DropColumn(
                name: "RowInFile",
                table: "Routes");
        }
    }
}
