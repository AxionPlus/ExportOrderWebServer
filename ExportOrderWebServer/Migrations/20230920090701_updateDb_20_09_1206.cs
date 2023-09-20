using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExportOrderWebServer.Migrations
{
    /// <inheritdoc />
    public partial class updateDb_20_09_1206 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CarrierDetails_Carriers_CarrierId",
                table: "CarrierDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_ExportOrders_VesselCalls_VesselCallId",
                table: "ExportOrders");

            migrationBuilder.AlterColumn<long>(
                name: "VesselCallId",
                table: "ExportOrders",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "CntrTareWt",
                table: "ExportOrder_Records",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UNNO",
                table: "DocumentRecord",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsIMO",
                table: "DocumentRecord",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "IMO",
                table: "DocumentRecord",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CommodityHSCode",
                table: "DocumentRecord",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TerminalName",
                table: "CarrierDetails",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Contract",
                table: "CarrierDetails",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "CarrierId",
                table: "CarrierDetails",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CarrierDetails_Carriers_CarrierId",
                table: "CarrierDetails",
                column: "CarrierId",
                principalTable: "Carriers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExportOrders_VesselCalls_VesselCallId",
                table: "ExportOrders",
                column: "VesselCallId",
                principalTable: "VesselCalls",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CarrierDetails_Carriers_CarrierId",
                table: "CarrierDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_ExportOrders_VesselCalls_VesselCallId",
                table: "ExportOrders");

            migrationBuilder.AlterColumn<long>(
                name: "VesselCallId",
                table: "ExportOrders",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<double>(
                name: "CntrTareWt",
                table: "ExportOrder_Records",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<string>(
                name: "UNNO",
                table: "DocumentRecord",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<bool>(
                name: "IsIMO",
                table: "DocumentRecord",
                type: "boolean",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<string>(
                name: "IMO",
                table: "DocumentRecord",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "CommodityHSCode",
                table: "DocumentRecord",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "TerminalName",
                table: "CarrierDetails",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Contract",
                table: "CarrierDetails",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<long>(
                name: "CarrierId",
                table: "CarrierDetails",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddForeignKey(
                name: "FK_CarrierDetails_Carriers_CarrierId",
                table: "CarrierDetails",
                column: "CarrierId",
                principalTable: "Carriers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ExportOrders_VesselCalls_VesselCallId",
                table: "ExportOrders",
                column: "VesselCallId",
                principalTable: "VesselCalls",
                principalColumn: "Id");
        }
    }
}
