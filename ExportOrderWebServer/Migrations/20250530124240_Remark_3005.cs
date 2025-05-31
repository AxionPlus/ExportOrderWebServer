using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExportOrderWebServer.Migrations
{
    /// <inheritdoc />
    public partial class Remark_3005 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {


            migrationBuilder.AddColumn<string>(
                name: "BillOfLadingNum",
                table: "ReleaseRemarks",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContainerNum",
                table: "ReleaseRemarks",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BillOfLadingNum",
                table: "ReleaseRemarks");

            migrationBuilder.DropColumn(
                name: "ContainerNum",
                table: "ReleaseRemarks");

        }
    }
}
