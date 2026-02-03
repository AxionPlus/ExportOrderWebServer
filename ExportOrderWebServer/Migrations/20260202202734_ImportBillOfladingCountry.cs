using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExportOrderWebServer.Migrations
{
    /// <inheritdoc />
    public partial class ImportBillOfladingCountry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Import_BillOfLadings_Import_Ports_PolId",
                table: "Import_BillOfLadings");

            migrationBuilder.AddColumn<string>(
                name: "GoogleTableUrl",
                table: "Import_VesselCalls",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "PolId",
                table: "Import_BillOfLadings",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "ConsigneeCountryRu",
                table: "Import_BillOfLadings",
                type: "text",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Import_BillOfLadings_Import_Ports_PolId",
                table: "Import_BillOfLadings",
                column: "PolId",
                principalTable: "Import_Ports",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Import_BillOfLadings_Import_Ports_PolId",
                table: "Import_BillOfLadings");

            migrationBuilder.DropColumn(
                name: "GoogleTableUrl",
                table: "Import_VesselCalls");

            migrationBuilder.DropColumn(
                name: "ConsigneeCountryRu",
                table: "Import_BillOfLadings");

            migrationBuilder.AlterColumn<Guid>(
                name: "PolId",
                table: "Import_BillOfLadings",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Import_BillOfLadings_Import_Ports_PolId",
                table: "Import_BillOfLadings",
                column: "PolId",
                principalTable: "Import_Ports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
