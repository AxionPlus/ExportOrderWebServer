using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExportOrderWebServer.Migrations
{
    /// <inheritdoc />
    public partial class VesselCallBase_add_TranslateUpdateTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CaptainLastName",
                table: "Import_VesselCalls",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CaptainName",
                table: "Import_VesselCalls",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "TranslateUpdateTime",
                table: "Import_VesselCalls",
                type: "timestamp without time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CaptainLastName",
                table: "Import_VesselCalls");

            migrationBuilder.DropColumn(
                name: "CaptainName",
                table: "Import_VesselCalls");

            migrationBuilder.DropColumn(
                name: "TranslateUpdateTime",
                table: "Import_VesselCalls");
        }
    }
}
