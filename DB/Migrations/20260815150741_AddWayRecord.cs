using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DB.Migrations
{
    /// <inheritdoc />
    public partial class AddWayRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WayRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StationFromId = table.Column<int>(type: "integer", nullable: false),
                    StationMiddleId = table.Column<int>(type: "integer", nullable: true),
                    StationToId = table.Column<int>(type: "integer", nullable: false),
                    wayLength = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WayRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WayRecords_Stations_StationFromId",
                        column: x => x.StationFromId,
                        principalTable: "Stations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WayRecords_Stations_StationMiddleId",
                        column: x => x.StationMiddleId,
                        principalTable: "Stations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WayRecords_Stations_StationToId",
                        column: x => x.StationToId,
                        principalTable: "Stations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WayRecords_StationFromId",
                table: "WayRecords",
                column: "StationFromId");

            migrationBuilder.CreateIndex(
                name: "IX_WayRecords_StationMiddleId",
                table: "WayRecords",
                column: "StationMiddleId");

            migrationBuilder.CreateIndex(
                name: "IX_WayRecords_StationToId",
                table: "WayRecords",
                column: "StationToId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WayRecords");
        }
    }
}
