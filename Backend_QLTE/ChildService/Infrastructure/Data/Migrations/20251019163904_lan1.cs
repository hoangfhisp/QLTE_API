using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend_QLTE.ChildService.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class lan1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChatMessages",
                columns: table => new
                {
                    MessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SenderId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReceiverId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LoaiTinNhan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThoiGian = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DaDoc = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessages", x => x.MessageId);
                });

            migrationBuilder.CreateTable(
                name: "Children",
                columns: table => new
                {
                    ChildId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NgaySinh = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GioiTinh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AnhDaiDien = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NhomTuoi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Children", x => x.ChildId);
                });

            migrationBuilder.CreateTable(
                name: "DangerZones",
                columns: table => new
                {
                    DangerZoneId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TenKhuVuc = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ViDo = table.Column<double>(type: "float", nullable: false),
                    KinhDo = table.Column<double>(type: "float", nullable: false),
                    BanKinh = table.Column<double>(type: "float", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DangerZones", x => x.DangerZoneId);
                });

            migrationBuilder.CreateTable(
                name: "SafeZones",
                columns: table => new
                {
                    SafeZoneId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TenZone = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ViDo = table.Column<double>(type: "float", nullable: false),
                    KinhDo = table.Column<double>(type: "float", nullable: false),
                    BanKinh = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SafeZones", x => x.SafeZoneId);
                });

            migrationBuilder.CreateTable(
                name: "Alerts",
                columns: table => new
                {
                    AlertId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChildId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoaiCanhBao = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThoiGian = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DaXuLy = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alerts", x => x.AlertId);
                    table.ForeignKey(
                        name: "FK_Alerts_Children_ChildId",
                        column: x => x.ChildId,
                        principalTable: "Children",
                        principalColumn: "ChildId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeviceInfos",
                columns: table => new
                {
                    DeviceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChildId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenThietBi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IMEI = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Pin = table.Column<int>(type: "int", nullable: true),
                    TrangThaiOnline = table.Column<bool>(type: "bit", nullable: false),
                    LanCapNhatCuoi = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceInfos", x => x.DeviceId);
                    table.ForeignKey(
                        name: "FK_DeviceInfos_Children_ChildId",
                        column: x => x.ChildId,
                        principalTable: "Children",
                        principalColumn: "ChildId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LocationHistories",
                columns: table => new
                {
                    LocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChildId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ViDo = table.Column<double>(type: "float", nullable: false),
                    KinhDo = table.Column<double>(type: "float", nullable: false),
                    ThoiGian = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DoChinhXac = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationHistories", x => x.LocationId);
                    table.ForeignKey(
                        name: "FK_LocationHistories_Children_ChildId",
                        column: x => x.ChildId,
                        principalTable: "Children",
                        principalColumn: "ChildId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SOSRequests",
                columns: table => new
                {
                    SOSId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChildId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ThoiGian = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ViDo = table.Column<double>(type: "float", nullable: false),
                    KinhDo = table.Column<double>(type: "float", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SOSRequests", x => x.SOSId);
                    table.ForeignKey(
                        name: "FK_SOSRequests_Children_ChildId",
                        column: x => x.ChildId,
                        principalTable: "Children",
                        principalColumn: "ChildId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_ChildId",
                table: "Alerts",
                column: "ChildId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceInfos_ChildId",
                table: "DeviceInfos",
                column: "ChildId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LocationHistories_ChildId",
                table: "LocationHistories",
                column: "ChildId");

            migrationBuilder.CreateIndex(
                name: "IX_SOSRequests_ChildId",
                table: "SOSRequests",
                column: "ChildId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Alerts");

            migrationBuilder.DropTable(
                name: "ChatMessages");

            migrationBuilder.DropTable(
                name: "DangerZones");

            migrationBuilder.DropTable(
                name: "DeviceInfos");

            migrationBuilder.DropTable(
                name: "LocationHistories");

            migrationBuilder.DropTable(
                name: "SafeZones");

            migrationBuilder.DropTable(
                name: "SOSRequests");

            migrationBuilder.DropTable(
                name: "Children");
        }
    }
}
