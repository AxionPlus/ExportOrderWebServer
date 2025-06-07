using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExportOrderWebServer.Migrations
{
    /// <inheritdoc />
    public partial class updateBillRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImoClass",
                table: "BillofLading_ContainerRecords",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Unno",
                table: "BillofLading_ContainerRecords",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImoClass",
                table: "BillofLading_ContainerRecords");

            migrationBuilder.DropColumn(
                name: "Unno",
                table: "BillofLading_ContainerRecords");
        }
    }
}
