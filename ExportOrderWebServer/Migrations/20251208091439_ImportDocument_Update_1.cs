using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExportOrderWebServer.Migrations
{
    /// <inheritdoc />
    public partial class ImportDocument_Update_1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TsPort",
                table: "Import_BillOfLadings",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<DateTime>(
                name: "TsDate",
                table: "Import_BillOfLadings",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AddColumn<string>(
                name: "CargoDescriptionRu",
                table: "Import_BillOfLadings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CargoDescriptionRu",
                table: "Import_BillOfLading_ContainerRecords",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CargoDescriptionRu",
                table: "Import_BillOfLadings");

            migrationBuilder.DropColumn(
                name: "CargoDescriptionRu",
                table: "Import_BillOfLading_ContainerRecords");

            migrationBuilder.AlterColumn<string>(
                name: "TsPort",
                table: "Import_BillOfLadings",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "TsDate",
                table: "Import_BillOfLadings",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);
        }
    }
}
