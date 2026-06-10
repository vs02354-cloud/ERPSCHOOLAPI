using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolERP.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddLeaveModuleStudentId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeaveRequests_Employees_EmployeeId",
                table: "LeaveRequests");

            migrationBuilder.AlterColumn<int>(
                name: "EmployeeId",
                table: "LeaveRequests",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "StudentId",
                table: "LeaveRequests",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_StudentId",
                table: "LeaveRequests",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveRequests_Employees_EmployeeId",
                table: "LeaveRequests",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveRequests_Students_StudentId",
                table: "LeaveRequests",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeaveRequests_Employees_EmployeeId",
                table: "LeaveRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_LeaveRequests_Students_StudentId",
                table: "LeaveRequests");

            migrationBuilder.DropIndex(
                name: "IX_LeaveRequests_StudentId",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "StudentId",
                table: "LeaveRequests");

            migrationBuilder.AlterColumn<int>(
                name: "EmployeeId",
                table: "LeaveRequests",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveRequests_Employees_EmployeeId",
                table: "LeaveRequests",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
