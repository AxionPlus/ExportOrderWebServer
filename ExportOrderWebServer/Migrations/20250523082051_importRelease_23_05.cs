using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExportOrderWebServer.Migrations
{
    /// <inheritdoc />
    public partial class importRelease_23_05 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LineName",
                table: "ReleaseContainerRecords",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TerminalName",
                table: "ReleaseContainerRecords",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LineName",
                table: "ReleaseContainerRecords");

            migrationBuilder.DropColumn(
                name: "TerminalName",
                table: "ReleaseContainerRecords");
        }
    }
}
