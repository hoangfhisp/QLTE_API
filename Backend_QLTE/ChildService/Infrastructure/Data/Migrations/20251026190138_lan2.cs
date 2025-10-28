using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend_QLTE.ChildService.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class lan2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeviceRestrictions",
                columns: table => new
                {
                    RestrictionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BlockedApps = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BlockedDomains = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AllowedDomains = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsFirewallEnabled = table.Column<bool>(type: "bit", nullable: false),
                    Mode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceRestrictions", x => x.RestrictionId);
                    table.ForeignKey(
                        name: "FK_DeviceRestrictions_DeviceInfos_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "DeviceInfos",
                        principalColumn: "DeviceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceRestrictions_DeviceId",
                table: "DeviceRestrictions",
                column: "DeviceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeviceRestrictions");
        }
    }
}
