using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DB.Migrations
{
    /// <inheritdoc />
    public partial class AddStationToMapCell : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StationId",
                table: "MapCells",
                type: "integer",
                nullable: true);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Stations_Name",
                table: "Stations",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_MapCells_StationId",
                table: "MapCells",
                column: "StationId");

            migrationBuilder.AddForeignKey(
                name: "FK_MapCells_Stations_StationId",
                table: "MapCells",
                column: "StationId",
                principalTable: "Stations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MapCells_Stations_StationId",
                table: "MapCells");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Stations_Name",
                table: "Stations");

            migrationBuilder.DropIndex(
                name: "IX_MapCells_StationId",
                table: "MapCells");

            migrationBuilder.DropColumn(
                name: "StationId",
                table: "MapCells");
        }
    }
}
