using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DB.Migrations
{
    /// <inheritdoc />
    public partial class AddSourseAndTargetStationForMapCell : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SourceStationId",
                table: "MapCells",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TargetStationId",
                table: "MapCells",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MapCells_SourceStationId",
                table: "MapCells",
                column: "SourceStationId");

            migrationBuilder.CreateIndex(
                name: "IX_MapCells_TargetStationId",
                table: "MapCells",
                column: "TargetStationId");

            migrationBuilder.AddForeignKey(
                name: "FK_MapCells_Stations_SourceStationId",
                table: "MapCells",
                column: "SourceStationId",
                principalTable: "Stations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MapCells_Stations_TargetStationId",
                table: "MapCells",
                column: "TargetStationId",
                principalTable: "Stations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MapCells_Stations_SourceStationId",
                table: "MapCells");

            migrationBuilder.DropForeignKey(
                name: "FK_MapCells_Stations_TargetStationId",
                table: "MapCells");

            migrationBuilder.DropIndex(
                name: "IX_MapCells_SourceStationId",
                table: "MapCells");

            migrationBuilder.DropIndex(
                name: "IX_MapCells_TargetStationId",
                table: "MapCells");

            migrationBuilder.DropColumn(
                name: "SourceStationId",
                table: "MapCells");

            migrationBuilder.DropColumn(
                name: "TargetStationId",
                table: "MapCells");
        }
    }
}
