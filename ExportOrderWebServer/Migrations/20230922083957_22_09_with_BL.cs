using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExportOrderWebServer.Migrations
{
    /// <inheritdoc />
    public partial class _22_09_with_BL : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "ContainerContent",
                newName: "PackageQty");

            migrationBuilder.AddColumn<string>(
                name: "PODAgent",
                table: "VesselCalls",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PackageName",
                table: "ContainerContent",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "blTemplate",
                table: "Carriers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Contract",
                table: "CarrierDetails",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "POLAgent",
                table: "CarrierDetails",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PODAgent",
                table: "VesselCalls");

            migrationBuilder.DropColumn(
                name: "PackageName",
                table: "ContainerContent");

            migrationBuilder.DropColumn(
                name: "blTemplate",
                table: "Carriers");

            migrationBuilder.DropColumn(
                name: "POLAgent",
                table: "CarrierDetails");

            migrationBuilder.RenameColumn(
                name: "PackageQty",
                table: "ContainerContent",
                newName: "Quantity");

            migrationBuilder.AlterColumn<string>(
                name: "Contract",
                table: "CarrierDetails",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
