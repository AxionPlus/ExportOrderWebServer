using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExportOrderWebServer.Migrations
{
    /// <inheritdoc />
    public partial class TerminalBaseEntity_ContractName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContractName",
                table: "Import_Terminals",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContractName",
                table: "Import_Terminals");
        }
    }
}
