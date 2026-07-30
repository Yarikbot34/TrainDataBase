using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DB.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Routes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    RouteNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Casual_Count = table.Column<int>(type: "integer", nullable: false),
                    Casual_Payment = table.Column<double>(type: "double precision", nullable: false),
                    Casual_WayLength = table.Column<double>(type: "double precision", nullable: false),
                    Casual_PaymentBySubject = table.Column<double>(type: "double precision", nullable: false),
                    Student_Count = table.Column<int>(type: "integer", nullable: false),
                    Student_Payment = table.Column<double>(type: "double precision", nullable: false),
                    Student_WayLength = table.Column<double>(type: "double precision", nullable: false),
                    Student_PaymentBySubject = table.Column<double>(type: "double precision", nullable: false),
                    FedBenefit_Count = table.Column<int>(type: "integer", nullable: false),
                    FedBenefit_Payment = table.Column<double>(type: "double precision", nullable: false),
                    FedBenefit_WayLength = table.Column<double>(type: "double precision", nullable: false),
                    FedBenefit_PaymentBySubject = table.Column<double>(type: "double precision", nullable: false),
                    RegBenefit_Count = table.Column<int>(type: "integer", nullable: false),
                    RegBenefit_Payment = table.Column<double>(type: "double precision", nullable: false),
                    RegBenefit_WayLength = table.Column<double>(type: "double precision", nullable: false),
                    RegBenefit_PaymentBySubject = table.Column<double>(type: "double precision", nullable: false),
                    Another_Count = table.Column<int>(type: "integer", nullable: false),
                    Another_Payment = table.Column<double>(type: "double precision", nullable: false),
                    Another_WayLength = table.Column<double>(type: "double precision", nullable: false),
                    Another_PaymentBySubject = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Routes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Stations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Class = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Trains",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Period = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RouteId = table.Column<int>(type: "integer", nullable: false),
                    StationFromId = table.Column<int>(type: "integer", nullable: false),
                    StationMiddleId = table.Column<int>(type: "integer", nullable: true),
                    StationToId = table.Column<int>(type: "integer", nullable: false),
                    TimeFrom = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    TimeTo = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    Distance = table.Column<int>(type: "integer", nullable: false),
                    RailcarCount = table.Column<int>(type: "integer", nullable: false),
                    DayInRaise = table.Column<int>(type: "integer", nullable: false),
                    RangePerDay = table.Column<int>(type: "integer", nullable: false),
                    RangePerMonth = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trains", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Trains_Routes_RouteId",
                        column: x => x.RouteId,
                        principalTable: "Routes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Trains_Stations_StationFromId",
                        column: x => x.StationFromId,
                        principalTable: "Stations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Trains_Stations_StationMiddleId",
                        column: x => x.StationMiddleId,
                        principalTable: "Stations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Trains_Stations_StationToId",
                        column: x => x.StationToId,
                        principalTable: "Stations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Trains_RouteId",
                table: "Trains",
                column: "RouteId");

            migrationBuilder.CreateIndex(
                name: "IX_Trains_StationFromId",
                table: "Trains",
                column: "StationFromId");

            migrationBuilder.CreateIndex(
                name: "IX_Trains_StationMiddleId",
                table: "Trains",
                column: "StationMiddleId");

            migrationBuilder.CreateIndex(
                name: "IX_Trains_StationToId",
                table: "Trains",
                column: "StationToId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Trains");

            migrationBuilder.DropTable(
                name: "Routes");

            migrationBuilder.DropTable(
                name: "Stations");
        }
    }
}
