using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExportOrderWebServer.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBillData_04_06_25 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConsigneeAddressRu",
                table: "BillofLadings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConsigneeCountryRu",
                table: "BillofLadings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConsigneeNameRu",
                table: "BillofLadings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CustomsDeliveryMode",
                table: "BillofLadings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ShipperAddressRu",
                table: "BillofLadings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShipperCountryRu",
                table: "BillofLadings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShipperNameRu",
                table: "BillofLadings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GoodsDescriptionRu",
                table: "BillofLading_ContainerRecords",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConsigneeAddressRu",
                table: "BillofLadings");

            migrationBuilder.DropColumn(
                name: "ConsigneeCountryRu",
                table: "BillofLadings");

            migrationBuilder.DropColumn(
                name: "ConsigneeNameRu",
                table: "BillofLadings");

            migrationBuilder.DropColumn(
                name: "CustomsDeliveryMode",
                table: "BillofLadings");

            migrationBuilder.DropColumn(
                name: "ShipperAddressRu",
                table: "BillofLadings");

            migrationBuilder.DropColumn(
                name: "ShipperCountryRu",
                table: "BillofLadings");

            migrationBuilder.DropColumn(
                name: "ShipperNameRu",
                table: "BillofLadings");

            migrationBuilder.DropColumn(
                name: "GoodsDescriptionRu",
                table: "BillofLading_ContainerRecords");
        }
    }
}
