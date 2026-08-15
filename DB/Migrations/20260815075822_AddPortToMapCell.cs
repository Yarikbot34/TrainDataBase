using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DB.Migrations
{
    /// <inheritdoc />
    public partial class AddPortToMapCell : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SourcePort",
                table: "MapCells",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TargetPort",
                table: "MapCells",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SourcePort",
                table: "MapCells");

            migrationBuilder.DropColumn(
                name: "TargetPort",
                table: "MapCells");
        }
    }
}
