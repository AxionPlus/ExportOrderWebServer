using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ExportOrderWebServer.Migrations
{
    /// <inheritdoc />
    public partial class ImportMode_19_05 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {


            migrationBuilder.CreateTable(
                name: "BillofLadings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: false),
                    Num = table.Column<string>(type: "text", nullable: true),
                    ServiceCode = table.Column<string>(type: "text", nullable: true),
                    IssueDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    SobDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ShipperName = table.Column<string>(type: "text", nullable: true),
                    ShipperAddress = table.Column<string>(type: "text", nullable: true),
                    ConsigneeName = table.Column<string>(type: "text", nullable: true),
                    ConsigneeAddress = table.Column<string>(type: "text", nullable: true),
                    ConsigneeTaxNo = table.Column<string>(type: "text", nullable: true),
                    NotifyName = table.Column<string>(type: "text", nullable: true),
                    NotifyAddress = table.Column<string>(type: "text", nullable: true),
                    NotifyEmail = table.Column<string>(type: "text", nullable: true),
                    AdditionalInfo = table.Column<string>(type: "text", nullable: true),
                    POR = table.Column<string>(type: "text", nullable: true),
                    POL = table.Column<string>(type: "text", nullable: true),
                    TS_PORT = table.Column<string>(type: "text", nullable: true),
                    POD = table.Column<string>(type: "text", nullable: true),
                    F_POD = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    LockToken = table.Column<long>(type: "bigint", nullable: false),
                    Timestamp = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillofLadings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReleaseContainerRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: false),
                    DocNumber = table.Column<string>(type: "text", nullable: false),
                    ReleaseUID = table.Column<string>(type: "text", nullable: false),
                    BillOfLadingNum = table.Column<string>(type: "text", nullable: false),
                    ContainerNum = table.Column<string>(type: "text", nullable: false),
                    ContainerType = table.Column<string>(type: "text", nullable: false),
                    ReleaseTo = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ReleaseMode = table.Column<int>(type: "integer", nullable: false),
                    ReleaseStatus = table.Column<int>(type: "integer", nullable: false),
                    TerminalErrorResponse = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    LockToken = table.Column<long>(type: "bigint", nullable: false),
                    Timestamp = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReleaseContainerRecords", x => x.Id);
                });

 

            migrationBuilder.CreateTable(
                name: "BillofLading_ContainerRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: false),
                    ContainerNum = table.Column<string>(type: "text", nullable: false),
                    ContainerType = table.Column<string>(type: "text", nullable: false),
                    IsSoc = table.Column<bool>(type: "boolean", nullable: false),
                    IsRef = table.Column<bool>(type: "boolean", nullable: false),
                    IsOog = table.Column<bool>(type: "boolean", nullable: false),
                    IsImo = table.Column<bool>(type: "boolean", nullable: false),
                    Seal = table.Column<string>(type: "text", nullable: true),
                    SealShr = table.Column<string>(type: "text", nullable: true),
                    SealOth = table.Column<string>(type: "text", nullable: true),
                    TareWeight = table.Column<int>(type: "integer", nullable: false),
                    CargoWeight = table.Column<double>(type: "double precision", nullable: false),
                    PackageQty = table.Column<int>(type: "integer", nullable: false),
                    IsAlcohol = table.Column<bool>(type: "boolean", nullable: false),
                    IsMilitaryCargo = table.Column<bool>(type: "boolean", nullable: false),
                    CommodityGroup = table.Column<string>(type: "text", nullable: true),
                    CommodityCode = table.Column<string>(type: "text", nullable: true),
                    BillofLadingEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    LockToken = table.Column<long>(type: "bigint", nullable: false),
                    Timestamp = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillofLading_ContainerRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BillofLading_ContainerRecords_BillofLadings_BillofLadingEnt~",
                        column: x => x.BillofLadingEntityId,
                        principalTable: "BillofLadings",
                        principalColumn: "Id");
                });


            migrationBuilder.CreateIndex(
                name: "IX_BillofLading_ContainerRecords_BillofLadingEntityId",
                table: "BillofLading_ContainerRecords",
                column: "BillofLadingEntityId");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {


        }
    }
}
