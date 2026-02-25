using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExportOrderWebServer.Migrations
{
    /// <inheritdoc />
    public partial class TerminalBaseEntity_ContractId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Import_BillOfLading_ContainerRecords_Import_BillOfLadings_B~",
                table: "Import_BillOfLading_ContainerRecords");

            migrationBuilder.AddColumn<string>(
                name: "ContractId",
                table: "Import_Terminals",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NameRu",
                table: "Import_Terminals",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<Guid>(
                name: "BillOfLadingBaseEntityId",
                table: "Import_BillOfLading_ContainerRecords",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Import_BillOfLading_ContainerRecords_Import_BillOfLadings_B~",
                table: "Import_BillOfLading_ContainerRecords",
                column: "BillOfLadingBaseEntityId",
                principalTable: "Import_BillOfLadings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Import_BillOfLading_ContainerRecords_Import_BillOfLadings_B~",
                table: "Import_BillOfLading_ContainerRecords");

            migrationBuilder.DropColumn(
                name: "ContractId",
                table: "Import_Terminals");

            migrationBuilder.DropColumn(
                name: "NameRu",
                table: "Import_Terminals");

            migrationBuilder.AlterColumn<Guid>(
                name: "BillOfLadingBaseEntityId",
                table: "Import_BillOfLading_ContainerRecords",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_Import_BillOfLading_ContainerRecords_Import_BillOfLadings_B~",
                table: "Import_BillOfLading_ContainerRecords",
                column: "BillOfLadingBaseEntityId",
                principalTable: "Import_BillOfLadings",
                principalColumn: "Id");
        }
    }
}
