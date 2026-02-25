using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExportOrderWebServer.Migrations
{
    /// <inheritdoc />
    public partial class PortOfLoadingDate_VesselCallBaseEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PortOfLoadingDate",
                table: "Import_VesselCalls",
                type: "timestamp without time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PortOfLoadingDate",
                table: "Import_VesselCalls");
        }
    }
}
