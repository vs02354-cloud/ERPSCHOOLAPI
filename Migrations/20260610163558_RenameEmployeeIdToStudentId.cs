using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolERP.Api.Migrations
{
    /// <inheritdoc />
    public partial class RenameEmployeeIdToStudentId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_LeaveRequests_Employees_EmployeeId]') AND parent_object_id = OBJECT_ID(N'[LeaveRequests]'))
                BEGIN
                    ALTER TABLE [LeaveRequests] DROP CONSTRAINT [FK_LeaveRequests_Employees_EmployeeId];
                END

                IF EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_LeaveRequests_EmployeeId' AND object_id = OBJECT_ID(N'[LeaveRequests]'))
                BEGIN
                    DROP INDEX [IX_LeaveRequests_EmployeeId] ON [LeaveRequests];
                END

                IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'EmployeeId' AND Object_ID = Object_ID(N'[LeaveRequests]'))
                BEGIN
                    EXEC sp_rename N'[LeaveRequests].[EmployeeId]', N'StudentId', N'COLUMN';
                END

                DECLARE @var0 sysname;
                SELECT @var0 = [d].[name]
                FROM [sys].[default_constraints] [d]
                INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
                WHERE ([d].[parent_object_id] = OBJECT_ID(N'[LeaveRequests]') AND [c].[name] = N'StudentId');
                IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [LeaveRequests] DROP CONSTRAINT [' + @var0 + '];');
                
                IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'StudentId' AND Object_ID = Object_ID(N'[LeaveRequests]'))
                BEGIN
                    ALTER TABLE [LeaveRequests] ALTER COLUMN [StudentId] int NULL;
                END

                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_LeaveRequests_StudentId' AND object_id = OBJECT_ID(N'[LeaveRequests]'))
                BEGIN
                    CREATE INDEX [IX_LeaveRequests_StudentId] ON [LeaveRequests] ([StudentId]);
                END

                IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_LeaveRequests_Students_StudentId]') AND parent_object_id = OBJECT_ID(N'[LeaveRequests]'))
                BEGIN
                    ALTER TABLE [LeaveRequests] ADD CONSTRAINT [FK_LeaveRequests_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Students] ([Id]);
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeaveRequests_Students_StudentId",
                table: "LeaveRequests");

            migrationBuilder.DropIndex(
                name: "IX_LeaveRequests_StudentId",
                table: "LeaveRequests");

            migrationBuilder.AlterColumn<int>(
                name: "StudentId",
                table: "LeaveRequests",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "LeaveRequests",
                newName: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_EmployeeId",
                table: "LeaveRequests",
                column: "EmployeeId");

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
