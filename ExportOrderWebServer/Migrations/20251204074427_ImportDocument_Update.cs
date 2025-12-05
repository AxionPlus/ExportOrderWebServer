using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExportOrderWebServer.Migrations
{
    /// <inheritdoc />
    public partial class ImportDocument_Update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Import_Bookings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Num = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockToken = table.Column<long>(type: "bigint", nullable: false),
                    Timestamp = table.Column<long>(type: "bigint", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    DeleteReason = table.Column<string>(type: "text", nullable: true),
                    HandledBySystem = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Import_Bookings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Import_Customers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    NameRu = table.Column<string>(type: "text", nullable: true),
                    AddressRu = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockToken = table.Column<long>(type: "bigint", nullable: false),
                    Timestamp = table.Column<long>(type: "bigint", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    DeleteReason = table.Column<string>(type: "text", nullable: true),
                    HandledBySystem = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Import_Customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Import_Ports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NameRu = table.Column<string>(type: "text", nullable: false),
                    NameEn = table.Column<string>(type: "text", nullable: false),
                    CountryRu = table.Column<string>(type: "text", nullable: false),
                    CountryEn = table.Column<string>(type: "text", nullable: false),
                    IsoCode = table.Column<string>(type: "text", nullable: false),
                    AuxIsoCode = table.Column<string>(type: "text", nullable: false),
                    PikYugIsoCode = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockToken = table.Column<long>(type: "bigint", nullable: false),
                    Timestamp = table.Column<long>(type: "bigint", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    DeleteReason = table.Column<string>(type: "text", nullable: true),
                    HandledBySystem = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Import_Ports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Import_Terminals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    CustomsPost = table.Column<string>(type: "text", nullable: false),
                    CustomsPostName = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockToken = table.Column<long>(type: "bigint", nullable: false),
                    Timestamp = table.Column<long>(type: "bigint", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    DeleteReason = table.Column<string>(type: "text", nullable: true),
                    HandledBySystem = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Import_Terminals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Import_Vessels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    FlagRu = table.Column<string>(type: "text", nullable: false),
                    FlagEn = table.Column<string>(type: "text", nullable: false),
                    ShortName = table.Column<string>(type: "text", nullable: false),
                    RolisCode = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockToken = table.Column<long>(type: "bigint", nullable: false),
                    Timestamp = table.Column<long>(type: "bigint", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    DeleteReason = table.Column<string>(type: "text", nullable: true),
                    HandledBySystem = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Import_Vessels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Import_VesselCalls",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VesselId = table.Column<Guid>(type: "uuid", nullable: false),
                    VoyageNo = table.Column<string>(type: "text", nullable: false),
                    TerminalVoyageNo = table.Column<string>(type: "text", nullable: false),
                    TerminalId = table.Column<Guid>(type: "uuid", nullable: false),
                    ETA = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ETS = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    PortOfLoadingId = table.Column<Guid>(type: "uuid", nullable: false),
                    FeederBlNo = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockToken = table.Column<long>(type: "bigint", nullable: false),
                    Timestamp = table.Column<long>(type: "bigint", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    DeleteReason = table.Column<string>(type: "text", nullable: true),
                    HandledBySystem = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Import_VesselCalls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Import_VesselCalls_Import_Ports_PortOfLoadingId",
                        column: x => x.PortOfLoadingId,
                        principalTable: "Import_Ports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Import_VesselCalls_Import_Terminals_TerminalId",
                        column: x => x.TerminalId,
                        principalTable: "Import_Terminals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Import_VesselCalls_Import_Vessels_VesselId",
                        column: x => x.VesselId,
                        principalTable: "Import_Vessels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Import_BillOfLadings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Num = table.Column<string>(type: "text", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    TsDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    TsPort = table.Column<string>(type: "text", nullable: false),
                    CustomerCode = table.Column<string>(type: "text", nullable: false),
                    BookingParty = table.Column<string>(type: "text", nullable: false),
                    Origin = table.Column<string>(type: "text", nullable: false),
                    Pol = table.Column<string>(type: "text", nullable: false),
                    Pod = table.Column<string>(type: "text", nullable: false),
                    FinalPod = table.Column<string>(type: "text", nullable: false),
                    Shipper = table.Column<string>(type: "text", nullable: false),
                    ShipperCode = table.Column<string>(type: "text", nullable: false),
                    ShipperName = table.Column<string>(type: "text", nullable: false),
                    ShipperAddress = table.Column<string>(type: "text", nullable: false),
                    ShipperNameRu = table.Column<string>(type: "text", nullable: true),
                    Consignee = table.Column<string>(type: "text", nullable: false),
                    ConsigneeCode = table.Column<string>(type: "text", nullable: false),
                    ConsigneeName = table.Column<string>(type: "text", nullable: false),
                    ConsigneeAddress = table.Column<string>(type: "text", nullable: false),
                    ConsigneeNameRu = table.Column<string>(type: "text", nullable: true),
                    ConsigneeAddressRu = table.Column<string>(type: "text", nullable: true),
                    PartBl = table.Column<string>(type: "text", nullable: false),
                    CargoDescription = table.Column<string>(type: "text", nullable: false),
                    VesselCallId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockToken = table.Column<long>(type: "bigint", nullable: false),
                    Timestamp = table.Column<long>(type: "bigint", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    DeleteReason = table.Column<string>(type: "text", nullable: true),
                    HandledBySystem = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Import_BillOfLadings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Import_BillOfLadings_Import_VesselCalls_VesselCallId",
                        column: x => x.VesselCallId,
                        principalTable: "Import_VesselCalls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Import_BillOfLading_ContainerRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContainerNo = table.Column<string>(type: "text", nullable: false),
                    ContainerTypeId = table.Column<string>(type: "text", nullable: false),
                    IsoCode = table.Column<string>(type: "text", nullable: false),
                    TareWt = table.Column<string>(type: "text", nullable: false),
                    FullOrEmpty = table.Column<string>(type: "text", nullable: false),
                    IsSoc = table.Column<bool>(type: "boolean", nullable: false),
                    SealNo = table.Column<string>(type: "text", nullable: false),
                    PackageType = table.Column<string>(type: "text", nullable: false),
                    NoOfPackage = table.Column<int>(type: "integer", nullable: false),
                    GrossWeight = table.Column<double>(type: "double precision", nullable: false),
                    GrossWeightUOM = table.Column<string>(type: "text", nullable: false),
                    Volume = table.Column<int>(type: "integer", nullable: false),
                    OutOfGauge = table.Column<bool>(type: "boolean", nullable: false),
                    IMCOClass = table.Column<string>(type: "text", nullable: true),
                    IMCONumber = table.Column<string>(type: "text", nullable: true),
                    ReeferTempSign = table.Column<string>(type: "text", nullable: true),
                    ReeferTemp = table.Column<string>(type: "text", nullable: true),
                    ReeferTempUOM = table.Column<string>(type: "text", nullable: true),
                    ReeferHumidity = table.Column<string>(type: "text", nullable: true),
                    ReeferVentilation = table.Column<string>(type: "text", nullable: true),
                    BookingNo = table.Column<string>(type: "text", nullable: false),
                    ContainerAsCargo = table.Column<bool>(type: "boolean", nullable: false),
                    BillOfLadingBaseEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockToken = table.Column<long>(type: "bigint", nullable: false),
                    Timestamp = table.Column<long>(type: "bigint", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    DeleteReason = table.Column<string>(type: "text", nullable: true),
                    HandledBySystem = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Import_BillOfLading_ContainerRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Import_BillOfLading_ContainerRecords_Import_BillOfLadings_B~",
                        column: x => x.BillOfLadingBaseEntityId,
                        principalTable: "Import_BillOfLadings",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Import_BillOfLading_ContainerRecords_BillOfLadingBaseEntity~",
                table: "Import_BillOfLading_ContainerRecords",
                column: "BillOfLadingBaseEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Import_BillOfLadings_VesselCallId",
                table: "Import_BillOfLadings",
                column: "VesselCallId");

            migrationBuilder.CreateIndex(
                name: "IX_Import_VesselCalls_PortOfLoadingId",
                table: "Import_VesselCalls",
                column: "PortOfLoadingId");

            migrationBuilder.CreateIndex(
                name: "IX_Import_VesselCalls_TerminalId",
                table: "Import_VesselCalls",
                column: "TerminalId");

            migrationBuilder.CreateIndex(
                name: "IX_Import_VesselCalls_VesselId",
                table: "Import_VesselCalls",
                column: "VesselId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Import_BillOfLading_ContainerRecords");

            migrationBuilder.DropTable(
                name: "Import_Bookings");

            migrationBuilder.DropTable(
                name: "Import_Customers");

            migrationBuilder.DropTable(
                name: "Import_BillOfLadings");

            migrationBuilder.DropTable(
                name: "Import_VesselCalls");

            migrationBuilder.DropTable(
                name: "Import_Ports");

            migrationBuilder.DropTable(
                name: "Import_Terminals");

            migrationBuilder.DropTable(
                name: "Import_Vessels");
        }
    }
}
