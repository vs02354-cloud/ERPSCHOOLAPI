using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolERP.Api.Migrations
{
    /// <inheritdoc />
    public partial class TransportManagementPhase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_TransportRoutes_Vehicles_VehicleId]') AND parent_object_id = OBJECT_ID(N'[dbo].[TransportRoutes]'))
                    ALTER TABLE [dbo].[TransportRoutes] DROP CONSTRAINT [FK_TransportRoutes_Vehicles_VehicleId];
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_TransportRoutes_VehicleId' AND object_id = OBJECT_ID(N'[dbo].[TransportRoutes]'))
                    DROP INDEX [IX_TransportRoutes_VehicleId] ON [dbo].[TransportRoutes];
            ");

            migrationBuilder.DropColumn(
                name: "VehicleId",
                table: "TransportRoutes");

            migrationBuilder.RenameColumn(
                name: "DriverName",
                table: "Vehicles",
                newName: "VehicleType");

            migrationBuilder.RenameColumn(
                name: "DriverContact",
                table: "Vehicles",
                newName: "RegistrationNumber");

            migrationBuilder.AddColumn<int>(
                name: "AssignedRouteId",
                table: "Vehicles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AttendantEmployeeId",
                table: "Vehicles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DriverEmployeeId",
                table: "Vehicles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FitnessExpiry",
                table: "Vehicles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InsuranceExpiry",
                table: "Vehicles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PermitDetails",
                table: "Vehicles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "PollutionExpiry",
                table: "Vehicles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TransportRouteId",
                table: "Vehicles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VehicleImage",
                table: "Vehicles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "EndTime",
                table: "TransportRoutes",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "TransportRoutes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RouteCode",
                table: "TransportRoutes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "StartTime",
                table: "TransportRoutes",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<int>(
                name: "TransportRouteStopId",
                table: "Students",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DriverLocations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VehicleId = table.Column<int>(type: "int", nullable: false),
                    DriverEmployeeId = table.Column<int>(type: "int", nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    SpeedKmH = table.Column<double>(type: "float", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DriverLocations_Employees_DriverEmployeeId",
                        column: x => x.DriverEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DriverLocations_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TransportAttendances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BoardingTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    DeboardingTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ScannedByEmployeeId = table.Column<int>(type: "int", nullable: true),
                    Method = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransportAttendances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransportAttendances_Employees_ScannedByEmployeeId",
                        column: x => x.ScannedByEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransportAttendances_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TransportGatePasses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    RouteId = table.Column<int>(type: "int", nullable: false),
                    VehicleId = table.Column<int>(type: "int", nullable: false),
                    QRCodeData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ValidUntil = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransportGatePasses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransportGatePasses_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransportGatePasses_TransportRoutes_RouteId",
                        column: x => x.RouteId,
                        principalTable: "TransportRoutes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransportGatePasses_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TransportRouteStops",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RouteId = table.Column<int>(type: "int", nullable: false),
                    StopName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    EstimatedArrivalTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    DistanceFromStart = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StopFare = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransportRouteStops", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransportRouteStops_TransportRoutes_RouteId",
                        column: x => x.RouteId,
                        principalTable: "TransportRoutes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_AssignedRouteId",
                table: "Vehicles",
                column: "AssignedRouteId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_AttendantEmployeeId",
                table: "Vehicles",
                column: "AttendantEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_DriverEmployeeId",
                table: "Vehicles",
                column: "DriverEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_TransportRouteId",
                table: "Vehicles",
                column: "TransportRouteId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_TransportRouteStopId",
                table: "Students",
                column: "TransportRouteStopId");

            migrationBuilder.CreateIndex(
                name: "IX_DriverLocations_DriverEmployeeId",
                table: "DriverLocations",
                column: "DriverEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_DriverLocations_VehicleId",
                table: "DriverLocations",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_TransportAttendances_ScannedByEmployeeId",
                table: "TransportAttendances",
                column: "ScannedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_TransportAttendances_StudentId",
                table: "TransportAttendances",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_TransportGatePasses_RouteId",
                table: "TransportGatePasses",
                column: "RouteId");

            migrationBuilder.CreateIndex(
                name: "IX_TransportGatePasses_StudentId",
                table: "TransportGatePasses",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_TransportGatePasses_VehicleId",
                table: "TransportGatePasses",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_TransportRouteStops_RouteId",
                table: "TransportRouteStops",
                column: "RouteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Students_TransportRouteStops_TransportRouteStopId",
                table: "Students",
                column: "TransportRouteStopId",
                principalTable: "TransportRouteStops",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_Employees_AttendantEmployeeId",
                table: "Vehicles",
                column: "AttendantEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_Employees_DriverEmployeeId",
                table: "Vehicles",
                column: "DriverEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_TransportRoutes_AssignedRouteId",
                table: "Vehicles",
                column: "AssignedRouteId",
                principalTable: "TransportRoutes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_TransportRoutes_TransportRouteId",
                table: "Vehicles",
                column: "TransportRouteId",
                principalTable: "TransportRoutes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Students_TransportRouteStops_TransportRouteStopId",
                table: "Students");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_Employees_AttendantEmployeeId",
                table: "Vehicles");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_Employees_DriverEmployeeId",
                table: "Vehicles");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_TransportRoutes_AssignedRouteId",
                table: "Vehicles");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_TransportRoutes_TransportRouteId",
                table: "Vehicles");

            migrationBuilder.DropTable(
                name: "DriverLocations");

            migrationBuilder.DropTable(
                name: "TransportAttendances");

            migrationBuilder.DropTable(
                name: "TransportGatePasses");

            migrationBuilder.DropTable(
                name: "TransportRouteStops");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_AssignedRouteId",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_AttendantEmployeeId",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_DriverEmployeeId",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_TransportRouteId",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Students_TransportRouteStopId",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "AssignedRouteId",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "AttendantEmployeeId",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "DriverEmployeeId",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "FitnessExpiry",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "InsuranceExpiry",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "PermitDetails",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "PollutionExpiry",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "TransportRouteId",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "VehicleImage",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "TransportRoutes");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "TransportRoutes");

            migrationBuilder.DropColumn(
                name: "RouteCode",
                table: "TransportRoutes");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "TransportRoutes");

            migrationBuilder.DropColumn(
                name: "TransportRouteStopId",
                table: "Students");

            migrationBuilder.RenameColumn(
                name: "VehicleType",
                table: "Vehicles",
                newName: "DriverName");

            migrationBuilder.RenameColumn(
                name: "RegistrationNumber",
                table: "Vehicles",
                newName: "DriverContact");

            migrationBuilder.AddColumn<int>(
                name: "VehicleId",
                table: "TransportRoutes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TransportRoutes_VehicleId",
                table: "TransportRoutes",
                column: "VehicleId");

            migrationBuilder.AddForeignKey(
                name: "FK_TransportRoutes_Vehicles_VehicleId",
                table: "TransportRoutes",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
