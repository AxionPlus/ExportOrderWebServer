using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ExportOrderWebServer.Migrations
{
    /// <inheritdoc />
    public partial class ImportVesselCall : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "VesselCallDetailId",
                table: "BillofLadings",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ImportVesselCalls",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VesselId = table.Column<long>(type: "bigint", nullable: false),
                    VoyageNo = table.Column<string>(type: "text", nullable: false),
                    VoyageNoTerminal = table.Column<string>(type: "text", nullable: false),
                    TerminalId = table.Column<long>(type: "bigint", nullable: false),
                    ETA = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ETS = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    CreateUserId = table.Column<string>(type: "text", nullable: true),
                    CreateTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportVesselCalls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImportVesselCalls_AspNetUsers_CreateUserId",
                        column: x => x.CreateUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ImportVesselCalls_Terminals_TerminalId",
                        column: x => x.TerminalId,
                        principalTable: "Terminals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ImportVesselCalls_Vessels_VesselId",
                        column: x => x.VesselId,
                        principalTable: "Vessels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ImportVesselCall_Details",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VesselCallId = table.Column<long>(type: "bigint", nullable: false),
                    PODId = table.Column<long>(type: "bigint", nullable: true),
                    FinalDestinationId = table.Column<long>(type: "bigint", nullable: true),
                    AgentPOD = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    CreateUserId = table.Column<string>(type: "text", nullable: true),
                    CreateTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportVesselCall_Details", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImportVesselCall_Details_AspNetUsers_CreateUserId",
                        column: x => x.CreateUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ImportVesselCall_Details_ImportVesselCalls_VesselCallId",
                        column: x => x.VesselCallId,
                        principalTable: "ImportVesselCalls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ImportVesselCall_Details_Locations_FinalDestinationId",
                        column: x => x.FinalDestinationId,
                        principalTable: "Locations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ImportVesselCall_Details_Locations_PODId",
                        column: x => x.PODId,
                        principalTable: "Locations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BillofLadings_VesselCallDetailId",
                table: "BillofLadings",
                column: "VesselCallDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportVesselCall_Details_CreateUserId",
                table: "ImportVesselCall_Details",
                column: "CreateUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportVesselCall_Details_FinalDestinationId",
                table: "ImportVesselCall_Details",
                column: "FinalDestinationId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportVesselCall_Details_PODId",
                table: "ImportVesselCall_Details",
                column: "PODId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportVesselCall_Details_VesselCallId",
                table: "ImportVesselCall_Details",
                column: "VesselCallId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportVesselCalls_CreateUserId",
                table: "ImportVesselCalls",
                column: "CreateUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportVesselCalls_TerminalId",
                table: "ImportVesselCalls",
                column: "TerminalId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportVesselCalls_VesselId",
                table: "ImportVesselCalls",
                column: "VesselId");

            migrationBuilder.AddForeignKey(
                name: "FK_BillofLadings_ImportVesselCall_Details_VesselCallDetailId",
                table: "BillofLadings",
                column: "VesselCallDetailId",
                principalTable: "ImportVesselCall_Details",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BillofLadings_ImportVesselCall_Details_VesselCallDetailId",
                table: "BillofLadings");

            migrationBuilder.DropTable(
                name: "ImportVesselCall_Details");

            migrationBuilder.DropTable(
                name: "ImportVesselCalls");

            migrationBuilder.DropIndex(
                name: "IX_BillofLadings_VesselCallDetailId",
                table: "BillofLadings");

            migrationBuilder.DropColumn(
                name: "VesselCallDetailId",
                table: "BillofLadings");
        }
    }
}
