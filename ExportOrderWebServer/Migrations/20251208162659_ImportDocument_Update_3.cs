using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExportOrderWebServer.Migrations
{
    /// <inheritdoc />
    public partial class ImportDocument_Update_3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Pol",
                table: "Import_BillOfLadings");

            migrationBuilder.RenameColumn(
                name: "TsPort",
                table: "Import_BillOfLadings",
                newName: "Carrier");

            migrationBuilder.AddColumn<Guid>(
                name: "PolId",
                table: "Import_BillOfLadings",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TsPortId",
                table: "Import_BillOfLadings",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Import_BillOfLadings_PolId",
                table: "Import_BillOfLadings",
                column: "PolId");

            migrationBuilder.CreateIndex(
                name: "IX_Import_BillOfLadings_TsPortId",
                table: "Import_BillOfLadings",
                column: "TsPortId");

            migrationBuilder.AddForeignKey(
                name: "FK_Import_BillOfLadings_Import_Ports_PolId",
                table: "Import_BillOfLadings",
                column: "PolId",
                principalTable: "Import_Ports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Import_BillOfLadings_Import_Ports_TsPortId",
                table: "Import_BillOfLadings",
                column: "TsPortId",
                principalTable: "Import_Ports",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Import_BillOfLadings_Import_Ports_PolId",
                table: "Import_BillOfLadings");

            migrationBuilder.DropForeignKey(
                name: "FK_Import_BillOfLadings_Import_Ports_TsPortId",
                table: "Import_BillOfLadings");

            migrationBuilder.DropIndex(
                name: "IX_Import_BillOfLadings_PolId",
                table: "Import_BillOfLadings");

            migrationBuilder.DropIndex(
                name: "IX_Import_BillOfLadings_TsPortId",
                table: "Import_BillOfLadings");

            migrationBuilder.DropColumn(
                name: "PolId",
                table: "Import_BillOfLadings");

            migrationBuilder.DropColumn(
                name: "TsPortId",
                table: "Import_BillOfLadings");

            migrationBuilder.RenameColumn(
                name: "Carrier",
                table: "Import_BillOfLadings",
                newName: "TsPort");

            migrationBuilder.AddColumn<string>(
                name: "Pol",
                table: "Import_BillOfLadings",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
