using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolERP.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateGatePassStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_TransportRoutes_TransportRouteId",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_TransportRouteId",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "TransportRouteId",
                table: "Vehicles");

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovalDate",
                table: "TransportGatePasses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedBy",
                table: "TransportGatePasses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TransportGatePasses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "TransportGatePasses",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Remarks",
                table: "TransportGatePasses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "TransportGatePasses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ValidityPeriodDays",
                table: "TransportGatePasses",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovalDate",
                table: "TransportGatePasses");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                table: "TransportGatePasses");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TransportGatePasses");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "TransportGatePasses");

            migrationBuilder.DropColumn(
                name: "Remarks",
                table: "TransportGatePasses");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "TransportGatePasses");

            migrationBuilder.DropColumn(
                name: "ValidityPeriodDays",
                table: "TransportGatePasses");

            migrationBuilder.AddColumn<int>(
                name: "TransportRouteId",
                table: "Vehicles",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_TransportRouteId",
                table: "Vehicles",
                column: "TransportRouteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_TransportRoutes_TransportRouteId",
                table: "Vehicles",
                column: "TransportRouteId",
                principalTable: "TransportRoutes",
                principalColumn: "Id");
        }
    }
}
