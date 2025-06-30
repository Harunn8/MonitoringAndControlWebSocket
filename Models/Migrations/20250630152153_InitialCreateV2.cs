using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Models.Migrations
{
    public partial class InitialCreateV2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Devices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceName = table.Column<string>(type: "text", nullable: true),
                    IpAddress = table.Column<string>(type: "text", nullable: true),
                    Port = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TcpDevices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceName = table.Column<string>(type: "text", nullable: true),
                    IpAddress = table.Column<string>(type: "text", nullable: true),
                    Port = table.Column<int>(type: "integer", nullable: false),
                    TcpFormat = table.Column<List<string>>(type: "text[]", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TcpDevices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: true),
                    Password = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OidMapping",
                columns: table => new
                {
                    ParameterId = table.Column<Guid>(type: "uuid", nullable: false),
                    Oid = table.Column<string>(type: "text", nullable: true),
                    ParameterName = table.Column<string>(type: "text", nullable: true),
                    SnmpDeviceId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OidMapping", x => x.ParameterId);
                    table.ForeignKey(
                        name: "FK_OidMapping_Devices_SnmpDeviceId",
                        column: x => x.SnmpDeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TcpDataV2",
                columns: table => new
                {
                    ParameterId = table.Column<Guid>(type: "uuid", nullable: false),
                    Request = table.Column<string>(type: "text", nullable: true),
                    ParameterName = table.Column<string>(type: "text", nullable: true),
                    TcpDeviceV2Id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TcpDataV2", x => x.ParameterId);
                    table.ForeignKey(
                        name: "FK_TcpDataV2_TcpDevices_TcpDeviceV2Id",
                        column: x => x.TcpDeviceV2Id,
                        principalTable: "TcpDevices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OidMapping_SnmpDeviceId",
                table: "OidMapping",
                column: "SnmpDeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_TcpDataV2_TcpDeviceV2Id",
                table: "TcpDataV2",
                column: "TcpDeviceV2Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OidMapping");

            migrationBuilder.DropTable(
                name: "TcpDataV2");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Devices");

            migrationBuilder.DropTable(
                name: "TcpDevices");
        }
    }
}
