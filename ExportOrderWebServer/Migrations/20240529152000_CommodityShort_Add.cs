using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ExportOrderWebServer.Migrations
{
    /// <inheritdoc />
    public partial class CommodityShort_Add : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DocumentHistory",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    ShipperName = table.Column<string>(type: "text", nullable: true),
                    ConsigneeName = table.Column<string>(type: "text", nullable: true),
                    Records = table.Column<string>(type: "jsonb", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Mode = table.Column<int>(type: "integer", nullable: false),
                    CreateUserId = table.Column<string>(type: "text", nullable: true),
                    CreateTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentHistory_AspNetUsers_CreateUserId",
                        column: x => x.CreateUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ExportOrderHistory",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Num = table.Column<string>(type: "text", nullable: true),
                    Dated = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Carrier = table.Column<string>(type: "text", nullable: true),
                    Person = table.Column<string>(type: "text", nullable: true),
                    VoyageNo = table.Column<string>(type: "text", nullable: true),
                    VesselName = table.Column<string>(type: "text", nullable: true),
                    POD = table.Column<string>(type: "text", nullable: true),
                    Documents = table.Column<string>(type: "jsonb", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Mode = table.Column<int>(type: "integer", nullable: false),
                    CreateUserId = table.Column<string>(type: "text", nullable: true),
                    CreateTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExportOrderHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExportOrderHistory_AspNetUsers_CreateUserId",
                        column: x => x.CreateUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "VesselCallHistory",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VesselName = table.Column<string>(type: "text", nullable: true),
                    VoyageNo = table.Column<string>(type: "text", nullable: true),
                    VoyageNoTerminal = table.Column<string>(type: "text", nullable: true),
                    TerminalName = table.Column<string>(type: "text", nullable: true),
                    ETA = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ETS = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Details = table.Column<string>(type: "jsonb", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Mode = table.Column<int>(type: "integer", nullable: false),
                    CreateUserId = table.Column<string>(type: "text", nullable: true),
                    CreateTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VesselCallHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VesselCallHistory_AspNetUsers_CreateUserId",
                        column: x => x.CreateUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ExportOrderRecordHistory",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CntrNum = table.Column<string>(type: "text", nullable: true),
                    CntrType = table.Column<string>(type: "text", nullable: true),
                    CntrTareWt = table.Column<double>(type: "double precision", nullable: false),
                    Seal = table.Column<string>(type: "text", nullable: true),
                    Contents = table.Column<string>(type: "jsonb", nullable: true),
                    ExportOrderId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExportOrderRecordHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExportOrderRecordHistory_ExportOrderHistory_ExportOrderId",
                        column: x => x.ExportOrderId,
                        principalTable: "ExportOrderHistory",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentHistory_CreateUserId",
                table: "DocumentHistory",
                column: "CreateUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ExportOrderHistory_CreateUserId",
                table: "ExportOrderHistory",
                column: "CreateUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ExportOrderRecordHistory_ExportOrderId",
                table: "ExportOrderRecordHistory",
                column: "ExportOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_VesselCallHistory_CreateUserId",
                table: "VesselCallHistory",
                column: "CreateUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentHistory");

            migrationBuilder.DropTable(
                name: "ExportOrderRecordHistory");

            migrationBuilder.DropTable(
                name: "VesselCallHistory");

            migrationBuilder.DropTable(
                name: "ExportOrderHistory");
        }
    }
}
