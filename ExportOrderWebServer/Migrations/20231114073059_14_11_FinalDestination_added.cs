using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ExportOrderWebServer.Migrations
{
    /// <inheritdoc />
    public partial class _14_11_FinalDestination_added : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContainerTypeSize",
                columns: table => new
                {
                    ISO = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    Normolize = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContainerTypeSize", x => x.ISO);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Commodities",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    NameEn = table.Column<string>(type: "text", nullable: false),
                    HSCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    IMO = table.Column<string>(type: "text", nullable: true),
                    UNNO = table.Column<string>(type: "text", nullable: true),
                    IsIMO = table.Column<bool>(type: "boolean", nullable: false),
                    CreateUserId = table.Column<string>(type: "text", nullable: true),
                    CreateTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Commodities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Commodities_AspNetUsers_CreateUserId",
                        column: x => x.CreateUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Countries",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RUS = table.Column<string>(type: "text", nullable: false),
                    ENG = table.Column<string>(type: "text", nullable: false),
                    CreateUserId = table.Column<string>(type: "text", nullable: true),
                    CreateTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Countries_AspNetUsers_CreateUserId",
                        column: x => x.CreateUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CustomsOffices",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Office = table.Column<string>(type: "text", nullable: true),
                    OfficeShort = table.Column<string>(type: "text", nullable: true),
                    Dapartment = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    CreateUserId = table.Column<string>(type: "text", nullable: true),
                    CreateTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomsOffices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomsOffices_AspNetUsers_CreateUserId",
                        column: x => x.CreateUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ContarctNo = table.Column<string>(type: "text", nullable: true),
                    Shipper = table.Column<string>(type: "jsonb", nullable: false),
                    Consignee = table.Column<string>(type: "jsonb", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    CreateUserId = table.Column<string>(type: "text", nullable: true),
                    CreateTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Documents_AspNetUsers_CreateUserId",
                        column: x => x.CreateUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MyCompany",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    CreateUserId = table.Column<string>(type: "text", nullable: true),
                    CreateTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MyCompany", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MyCompany_AspNetUsers_CreateUserId",
                        column: x => x.CreateUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    NameEn = table.Column<string>(type: "text", nullable: true),
                    CountryId = table.Column<long>(type: "bigint", nullable: true),
                    CreateUserId = table.Column<string>(type: "text", nullable: true),
                    CreateTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Customers_AspNetUsers_CreateUserId",
                        column: x => x.CreateUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Customers_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    NameEn = table.Column<string>(type: "text", nullable: true),
                    UnLocode = table.Column<string>(type: "text", nullable: true),
                    CountryId = table.Column<long>(type: "bigint", nullable: true),
                    CreateUserId = table.Column<string>(type: "text", nullable: true),
                    CreateTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Locations_AspNetUsers_CreateUserId",
                        column: x => x.CreateUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Locations_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Vessels",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IMO = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    TerminalCode = table.Column<string>(type: "text", nullable: true),
                    FlagId = table.Column<long>(type: "bigint", nullable: true),
                    CaptainFamily = table.Column<string>(type: "text", nullable: true),
                    CaptainName = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    CreateUserId = table.Column<string>(type: "text", nullable: false),
                    CreateTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vessels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vessels_AspNetUsers_CreateUserId",
                        column: x => x.CreateUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Vessels_Countries_FlagId",
                        column: x => x.FlagId,
                        principalTable: "Countries",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DocumentRecord",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Seq = table.Column<int>(type: "integer", nullable: false),
                    CommodityName = table.Column<string>(type: "text", nullable: false),
                    CommodityEngName = table.Column<string>(type: "text", nullable: false),
                    CommodityHSCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    IMO = table.Column<string>(type: "text", nullable: true),
                    UNNO = table.Column<string>(type: "text", nullable: true),
                    IsIMO = table.Column<bool>(type: "boolean", nullable: false),
                    NetWt = table.Column<double>(type: "double precision", nullable: false),
                    GrossWt = table.Column<double>(type: "double precision", nullable: false),
                    Volume = table.Column<double>(type: "double precision", nullable: true),
                    DocumentId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentRecord", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentRecord_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Persons",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    SurName = table.Column<string>(type: "text", nullable: true),
                    FamilyName = table.Column<string>(type: "text", nullable: true),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    BirthYear = table.Column<string>(type: "text", nullable: true),
                    BirthPlace = table.Column<string>(type: "text", nullable: true),
                    Company = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    Passport = table.Column<string>(type: "text", nullable: true),
                    MyCompanyEntityId = table.Column<long>(type: "bigint", nullable: true),
                    CreateUserId = table.Column<string>(type: "text", nullable: true),
                    CreateTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Persons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Persons_AspNetUsers_CreateUserId",
                        column: x => x.CreateUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Persons_MyCompany_MyCompanyEntityId",
                        column: x => x.MyCompanyEntityId,
                        principalTable: "MyCompany",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Carriers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    NameEn = table.Column<string>(type: "text", nullable: true),
                    BlTemplate = table.Column<int>(type: "integer", nullable: false),
                    LocationId = table.Column<long>(type: "bigint", nullable: true),
                    CreateUserId = table.Column<string>(type: "text", nullable: true),
                    CreateTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Carriers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Carriers_AspNetUsers_CreateUserId",
                        column: x => x.CreateUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Carriers_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Terminals",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    LocationId = table.Column<long>(type: "bigint", nullable: true),
                    CustomsId = table.Column<long>(type: "bigint", nullable: true),
                    CreateUserId = table.Column<string>(type: "text", nullable: true),
                    CreateTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Terminals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Terminals_AspNetUsers_CreateUserId",
                        column: x => x.CreateUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Terminals_CustomsOffices_CustomsId",
                        column: x => x.CustomsId,
                        principalTable: "CustomsOffices",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Terminals_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CarrierDetails",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TerminalName = table.Column<string>(type: "text", nullable: false),
                    Contract = table.Column<string>(type: "text", nullable: true),
                    DateContract = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    AgentPOL = table.Column<string>(type: "text", nullable: true),
                    CarrierId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarrierDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarrierDetails_Carriers_CarrierId",
                        column: x => x.CarrierId,
                        principalTable: "Carriers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VesselCalls",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VesselId = table.Column<long>(type: "bigint", nullable: false),
                    VoyageNo = table.Column<string>(type: "text", nullable: false),
                    VoyageNoTerminal = table.Column<string>(type: "text", nullable: false),
                    TerminalId = table.Column<long>(type: "bigint", nullable: false),
                    ETA = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ETS = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    CreateUserId = table.Column<string>(type: "text", nullable: true),
                    CreateTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VesselCalls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VesselCalls_AspNetUsers_CreateUserId",
                        column: x => x.CreateUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VesselCalls_Terminals_TerminalId",
                        column: x => x.TerminalId,
                        principalTable: "Terminals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VesselCalls_Vessels_VesselId",
                        column: x => x.VesselId,
                        principalTable: "Vessels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VesselCall_Details",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VesselCallId = table.Column<long>(type: "bigint", nullable: false),
                    PODId = table.Column<long>(type: "bigint", nullable: true),
                    FinalDestinationId = table.Column<long>(type: "bigint", nullable: true),
                    AgentPOD = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    CreateUserId = table.Column<string>(type: "text", nullable: true),
                    CreateTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VesselCall_Details", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VesselCall_Details_AspNetUsers_CreateUserId",
                        column: x => x.CreateUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VesselCall_Details_Locations_FinalDestinationId",
                        column: x => x.FinalDestinationId,
                        principalTable: "Locations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VesselCall_Details_Locations_PODId",
                        column: x => x.PODId,
                        principalTable: "Locations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VesselCall_Details_VesselCalls_VesselCallId",
                        column: x => x.VesselCallId,
                        principalTable: "VesselCalls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExportOrders",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Num = table.Column<string>(type: "text", nullable: false),
                    Dated = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CarrierId = table.Column<long>(type: "bigint", nullable: true),
                    PersonId = table.Column<long>(type: "bigint", nullable: true),
                    VesselCallDetailId = table.Column<long>(type: "bigint", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    CreateUserId = table.Column<string>(type: "text", nullable: true),
                    CreateTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExportOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExportOrders_AspNetUsers_CreateUserId",
                        column: x => x.CreateUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ExportOrders_Carriers_CarrierId",
                        column: x => x.CarrierId,
                        principalTable: "Carriers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ExportOrders_Persons_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Persons",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ExportOrders_VesselCall_Details_VesselCallDetailId",
                        column: x => x.VesselCallDetailId,
                        principalTable: "VesselCall_Details",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ExportOrder_Records",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CntrNum = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    CntrTypeISO = table.Column<string>(type: "character varying(4)", nullable: true),
                    CntrTareWt = table.Column<double>(type: "double precision", nullable: false),
                    Seal = table.Column<string>(type: "text", nullable: false),
                    ExportOrderId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExportOrder_Records", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExportOrder_Records_ContainerTypeSize_CntrTypeISO",
                        column: x => x.CntrTypeISO,
                        principalTable: "ContainerTypeSize",
                        principalColumn: "ISO");
                    table.ForeignKey(
                        name: "FK_ExportOrder_Records_ExportOrders_ExportOrderId",
                        column: x => x.ExportOrderId,
                        principalTable: "ExportOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExportOrders_Documents",
                columns: table => new
                {
                    DocumentsId = table.Column<long>(type: "bigint", nullable: false),
                    ExportOrdersId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExportOrders_Documents", x => new { x.DocumentsId, x.ExportOrdersId });
                    table.ForeignKey(
                        name: "FK_ExportOrders_Documents_Documents_DocumentsId",
                        column: x => x.DocumentsId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExportOrders_Documents_ExportOrders_ExportOrdersId",
                        column: x => x.ExportOrdersId,
                        principalTable: "ExportOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContainerContent",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PackageQty = table.Column<long>(type: "bigint", nullable: true),
                    PackageName = table.Column<string>(type: "text", nullable: true),
                    NetWt = table.Column<double>(type: "double precision", nullable: true),
                    GrossWt = table.Column<double>(type: "double precision", nullable: true),
                    Volume = table.Column<double>(type: "double precision", nullable: true),
                    DocumentRecordId = table.Column<int>(type: "integer", nullable: false),
                    ExportOrderRecordId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContainerContent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContainerContent_DocumentRecord_DocumentRecordId",
                        column: x => x.DocumentRecordId,
                        principalTable: "DocumentRecord",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContainerContent_ExportOrder_Records_ExportOrderRecordId",
                        column: x => x.ExportOrderRecordId,
                        principalTable: "ExportOrder_Records",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CarrierDetails_CarrierId",
                table: "CarrierDetails",
                column: "CarrierId");

            migrationBuilder.CreateIndex(
                name: "IX_Carriers_CreateUserId",
                table: "Carriers",
                column: "CreateUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Carriers_LocationId",
                table: "Carriers",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Commodities_CreateUserId",
                table: "Commodities",
                column: "CreateUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ContainerContent_DocumentRecordId",
                table: "ContainerContent",
                column: "DocumentRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_ContainerContent_ExportOrderRecordId",
                table: "ContainerContent",
                column: "ExportOrderRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_Countries_CreateUserId",
                table: "Countries",
                column: "CreateUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_CountryId",
                table: "Customers",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_CreateUserId",
                table: "Customers",
                column: "CreateUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomsOffices_CreateUserId",
                table: "CustomsOffices",
                column: "CreateUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentRecord_DocumentId",
                table: "DocumentRecord",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_CreateUserId",
                table: "Documents",
                column: "CreateUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ExportOrder_Records_CntrTypeISO",
                table: "ExportOrder_Records",
                column: "CntrTypeISO");

            migrationBuilder.CreateIndex(
                name: "IX_ExportOrder_Records_ExportOrderId",
                table: "ExportOrder_Records",
                column: "ExportOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ExportOrders_CarrierId",
                table: "ExportOrders",
                column: "CarrierId");

            migrationBuilder.CreateIndex(
                name: "IX_ExportOrders_CreateUserId",
                table: "ExportOrders",
                column: "CreateUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ExportOrders_PersonId",
                table: "ExportOrders",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_ExportOrders_VesselCallDetailId",
                table: "ExportOrders",
                column: "VesselCallDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_ExportOrders_Documents_ExportOrdersId",
                table: "ExportOrders_Documents",
                column: "ExportOrdersId");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_CountryId",
                table: "Locations",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_CreateUserId",
                table: "Locations",
                column: "CreateUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MyCompany_CreateUserId",
                table: "MyCompany",
                column: "CreateUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Persons_CreateUserId",
                table: "Persons",
                column: "CreateUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Persons_MyCompanyEntityId",
                table: "Persons",
                column: "MyCompanyEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Terminals_CreateUserId",
                table: "Terminals",
                column: "CreateUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Terminals_CustomsId",
                table: "Terminals",
                column: "CustomsId");

            migrationBuilder.CreateIndex(
                name: "IX_Terminals_LocationId",
                table: "Terminals",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_VesselCall_Details_CreateUserId",
                table: "VesselCall_Details",
                column: "CreateUserId");

            migrationBuilder.CreateIndex(
                name: "IX_VesselCall_Details_FinalDestinationId",
                table: "VesselCall_Details",
                column: "FinalDestinationId");

            migrationBuilder.CreateIndex(
                name: "IX_VesselCall_Details_PODId",
                table: "VesselCall_Details",
                column: "PODId");

            migrationBuilder.CreateIndex(
                name: "IX_VesselCall_Details_VesselCallId",
                table: "VesselCall_Details",
                column: "VesselCallId");

            migrationBuilder.CreateIndex(
                name: "IX_VesselCalls_CreateUserId",
                table: "VesselCalls",
                column: "CreateUserId");

            migrationBuilder.CreateIndex(
                name: "IX_VesselCalls_TerminalId",
                table: "VesselCalls",
                column: "TerminalId");

            migrationBuilder.CreateIndex(
                name: "IX_VesselCalls_VesselId",
                table: "VesselCalls",
                column: "VesselId");

            migrationBuilder.CreateIndex(
                name: "IX_Vessels_CreateUserId",
                table: "Vessels",
                column: "CreateUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Vessels_FlagId",
                table: "Vessels",
                column: "FlagId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "CarrierDetails");

            migrationBuilder.DropTable(
                name: "Commodities");

            migrationBuilder.DropTable(
                name: "ContainerContent");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "ExportOrders_Documents");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "DocumentRecord");

            migrationBuilder.DropTable(
                name: "ExportOrder_Records");

            migrationBuilder.DropTable(
                name: "Documents");

            migrationBuilder.DropTable(
                name: "ContainerTypeSize");

            migrationBuilder.DropTable(
                name: "ExportOrders");

            migrationBuilder.DropTable(
                name: "Carriers");

            migrationBuilder.DropTable(
                name: "Persons");

            migrationBuilder.DropTable(
                name: "VesselCall_Details");

            migrationBuilder.DropTable(
                name: "MyCompany");

            migrationBuilder.DropTable(
                name: "VesselCalls");

            migrationBuilder.DropTable(
                name: "Terminals");

            migrationBuilder.DropTable(
                name: "Vessels");

            migrationBuilder.DropTable(
                name: "CustomsOffices");

            migrationBuilder.DropTable(
                name: "Locations");

            migrationBuilder.DropTable(
                name: "Countries");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
