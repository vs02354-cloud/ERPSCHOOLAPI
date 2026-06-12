using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolERP.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddTransportAndCommission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReferredByEmployeeId",
                table: "Students",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IncludesTransportFee",
                table: "FeePayments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "TransportFeeAmount",
                table: "FeePayments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "CommissionSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CommissionType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CommissionValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommissionSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TeacherCommissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    FeePaymentId = table.Column<int>(type: "int", nullable: false),
                    CommissionAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DateEarned = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsPaid = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherCommissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeacherCommissions_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TeacherCommissions_FeePayments_FeePaymentId",
                        column: x => x.FeePaymentId,
                        principalTable: "FeePayments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TeacherCommissions_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Students_ReferredByEmployeeId",
                table: "Students",
                column: "ReferredByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherCommissions_EmployeeId",
                table: "TeacherCommissions",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherCommissions_FeePaymentId",
                table: "TeacherCommissions",
                column: "FeePaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherCommissions_StudentId",
                table: "TeacherCommissions",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Employees_ReferredByEmployeeId",
                table: "Students",
                column: "ReferredByEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Students_Employees_ReferredByEmployeeId",
                table: "Students");

            migrationBuilder.DropTable(
                name: "CommissionSettings");

            migrationBuilder.DropTable(
                name: "TeacherCommissions");

            migrationBuilder.DropIndex(
                name: "IX_Students_ReferredByEmployeeId",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "ReferredByEmployeeId",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "IncludesTransportFee",
                table: "FeePayments");

            migrationBuilder.DropColumn(
                name: "TransportFeeAmount",
                table: "FeePayments");
        }
    }
}
