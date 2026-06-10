BEGIN TRANSACTION;
GO

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_LeaveRequests_Employees_EmployeeId]') AND parent_object_id = OBJECT_ID(N'[LeaveRequests]'))
BEGIN
    ALTER TABLE [LeaveRequests] DROP CONSTRAINT [FK_LeaveRequests_Employees_EmployeeId];
END
GO

IF EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_LeaveRequests_EmployeeId' AND object_id = OBJECT_ID(N'[LeaveRequests]'))
BEGIN
    DROP INDEX [IX_LeaveRequests_EmployeeId] ON [LeaveRequests];
END
GO

EXEC sp_rename N'[LeaveRequests].[EmployeeId]', N'StudentId', N'COLUMN';
GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[LeaveRequests]') AND [c].[name] = N'StudentId');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [LeaveRequests] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [LeaveRequests] ALTER COLUMN [StudentId] int NULL;
GO

CREATE INDEX [IX_LeaveRequests_StudentId] ON [LeaveRequests] ([StudentId]);
GO

ALTER TABLE [LeaveRequests] ADD CONSTRAINT [FK_LeaveRequests_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Students] ([Id]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260610163558_RenameEmployeeIdToStudentId', N'8.0.4');
GO

COMMIT;
GO

