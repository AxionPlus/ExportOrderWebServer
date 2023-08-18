using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ExportOrderWebServer.Migrations
{
    /// <inheritdoc />
    public partial class vessel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Vessels",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IMO = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    TerminalId = table.Column<string>(type: "text", nullable: false),
                    FlagId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Version = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: false),
                    CreateUserId = table.Column<string>(type: "text", nullable: false),
                    CreateTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vessels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vessels_AspNetUsers_CreateUserId",
                        column: x => x.CreateUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Vessels_Countries_FlagId",
                        column: x => x.FlagId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VesselCalls",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VesselId = table.Column<long>(type: "bigint", nullable: false),
                    VoyageCarrier = table.Column<string>(type: "text", nullable: false),
                    VoyageTerminal = table.Column<string>(type: "text", nullable: false),
                    LoadingTerminalId = table.Column<long>(type: "bigint", nullable: false),
                    PODId = table.Column<long>(type: "bigint", nullable: false),
                    ETA = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ETS = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Version = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: false),
                    CreateUserId = table.Column<string>(type: "text", nullable: false),
                    CreateTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VesselCalls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VesselCalls_AspNetUsers_CreateUserId",
                        column: x => x.CreateUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VesselCalls_Locations_PODId",
                        column: x => x.PODId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VesselCalls_Terminals_LoadingTerminalId",
                        column: x => x.LoadingTerminalId,
                        principalTable: "Terminals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VesselCalls_Vessels_VesselId",
                        column: x => x.VesselId,
                        principalTable: "Vessels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VesselCalls_CreateUserId",
                table: "VesselCalls",
                column: "CreateUserId");

            migrationBuilder.CreateIndex(
                name: "IX_VesselCalls_LoadingTerminalId",
                table: "VesselCalls",
                column: "LoadingTerminalId");

            migrationBuilder.CreateIndex(
                name: "IX_VesselCalls_PODId",
                table: "VesselCalls",
                column: "PODId");

            migrationBuilder.CreateIndex(
                name: "IX_VesselCalls_VesselId",
                table: "VesselCalls",
                column: "VesselId");

            migrationBuilder.CreateIndex(
                name: "IX_Vessels_CreateUserId",
                table: "Vessels",
                column: "CreateUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Vessels_FlagId",
                table: "Vessels",
                column: "FlagId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VesselCalls");

            migrationBuilder.DropTable(
                name: "Vessels");
        }
    }
}
