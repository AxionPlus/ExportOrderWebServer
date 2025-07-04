using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExportOrderWebServer.Migrations
{
    /// <inheritdoc />
    public partial class undapeManePOL : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ImportVesselCall_Details_Locations_PODId",
                table: "ImportVesselCall_Details");

            migrationBuilder.RenameColumn(
                name: "PODId",
                table: "ImportVesselCall_Details",
                newName: "POLId");

            migrationBuilder.RenameColumn(
                name: "AgentPOD",
                table: "ImportVesselCall_Details",
                newName: "AgentPOL");

            migrationBuilder.RenameIndex(
                name: "IX_ImportVesselCall_Details_PODId",
                table: "ImportVesselCall_Details",
                newName: "IX_ImportVesselCall_Details_POLId");

            migrationBuilder.AddForeignKey(
                name: "FK_ImportVesselCall_Details_Locations_POLId",
                table: "ImportVesselCall_Details",
                column: "POLId",
                principalTable: "Locations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ImportVesselCall_Details_Locations_POLId",
                table: "ImportVesselCall_Details");

            migrationBuilder.RenameColumn(
                name: "POLId",
                table: "ImportVesselCall_Details",
                newName: "PODId");

            migrationBuilder.RenameColumn(
                name: "AgentPOL",
                table: "ImportVesselCall_Details",
                newName: "AgentPOD");

            migrationBuilder.RenameIndex(
                name: "IX_ImportVesselCall_Details_POLId",
                table: "ImportVesselCall_Details",
                newName: "IX_ImportVesselCall_Details_PODId");

          

            migrationBuilder.AddForeignKey(
                name: "FK_ImportVesselCall_Details_Locations_PODId",
                table: "ImportVesselCall_Details",
                column: "PODId",
                principalTable: "Locations",
                principalColumn: "Id");
        }
    }
}
