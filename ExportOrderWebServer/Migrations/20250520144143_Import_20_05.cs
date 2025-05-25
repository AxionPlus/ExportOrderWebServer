using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExportOrderWebServer.Migrations
{
    /// <inheritdoc />
    public partial class Import_20_05 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Seal",
                table: "BillofLading_ContainerRecords",
                newName: "SealNo");

            migrationBuilder.AddColumn<string>(
                name: "GoodsDescription",
                table: "BillofLading_ContainerRecords",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TempSet",
                table: "BillofLading_ContainerRecords",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GoodsDescription",
                table: "BillofLading_ContainerRecords");

            migrationBuilder.DropColumn(
                name: "TempSet",
                table: "BillofLading_ContainerRecords");

            migrationBuilder.RenameColumn(
                name: "SealNo",
                table: "BillofLading_ContainerRecords",
                newName: "Seal");
        }
    }
}
