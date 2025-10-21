using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend_QLTE.ChildService.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class addchildrenIdZone1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ChildrenId",
                table: "SafeZones",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ChildrenId",
                table: "DangerZones",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SafeZones_ChildrenId",
                table: "SafeZones",
                column: "ChildrenId");

            migrationBuilder.CreateIndex(
                name: "IX_DangerZones_ChildrenId",
                table: "DangerZones",
                column: "ChildrenId");

            migrationBuilder.AddForeignKey(
                name: "FK_DangerZones_Children_ChildrenId",
                table: "DangerZones",
                column: "ChildrenId",
                principalTable: "Children",
                principalColumn: "ChildId");

            migrationBuilder.AddForeignKey(
                name: "FK_SafeZones_Children_ChildrenId",
                table: "SafeZones",
                column: "ChildrenId",
                principalTable: "Children",
                principalColumn: "ChildId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DangerZones_Children_ChildrenId",
                table: "DangerZones");

            migrationBuilder.DropForeignKey(
                name: "FK_SafeZones_Children_ChildrenId",
                table: "SafeZones");

            migrationBuilder.DropIndex(
                name: "IX_SafeZones_ChildrenId",
                table: "SafeZones");

            migrationBuilder.DropIndex(
                name: "IX_DangerZones_ChildrenId",
                table: "DangerZones");

            migrationBuilder.DropColumn(
                name: "ChildrenId",
                table: "SafeZones");

            migrationBuilder.DropColumn(
                name: "ChildrenId",
                table: "DangerZones");
        }
    }
}
