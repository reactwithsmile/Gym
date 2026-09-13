using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymApi.Data.Migrations;

public partial class AddPaymentAuditFields : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF COL_LENGTH(N'[Payments]', N'CreatedAt') IS NULL
                ALTER TABLE [Payments] ADD [CreatedAt] datetime2 NOT NULL CONSTRAINT [DF_Payments_CreatedAt] DEFAULT ('0001-01-01');
            IF COL_LENGTH(N'[Payments]', N'CreatedByUserId') IS NULL
                ALTER TABLE [Payments] ADD [CreatedByUserId] int NULL;
            IF COL_LENGTH(N'[Payments]', N'Notes') IS NULL
                ALTER TABLE [Payments] ADD [Notes] nvarchar(1000) NOT NULL CONSTRAINT [DF_Payments_Notes] DEFAULT ('');
            IF COL_LENGTH(N'[Payments]', N'Status') IS NULL
                ALTER TABLE [Payments] ADD [Status] nvarchar(32) NOT NULL CONSTRAINT [DF_Payments_Status] DEFAULT ('Paid');
            IF COL_LENGTH(N'[Payments]', N'UpdatedAt') IS NULL
                ALTER TABLE [Payments] ADD [UpdatedAt] datetime2 NOT NULL CONSTRAINT [DF_Payments_UpdatedAt] DEFAULT ('0001-01-01');
            IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Payments_CreatedByUserId' AND object_id = OBJECT_ID(N'[Payments]'))
                CREATE INDEX [IX_Payments_CreatedByUserId] ON [Payments] ([CreatedByUserId]);
            IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Payments_Users_CreatedByUserId')
                ALTER TABLE [Payments] ADD CONSTRAINT [FK_Payments_Users_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [Users] ([Id]) ON DELETE SET NULL;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Payments_Users_CreatedByUserId')
                ALTER TABLE [Payments] DROP CONSTRAINT [FK_Payments_Users_CreatedByUserId];
            IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Payments_CreatedByUserId' AND object_id = OBJECT_ID(N'[Payments]'))
                DROP INDEX [IX_Payments_CreatedByUserId] ON [Payments];
            IF COL_LENGTH(N'[Payments]', N'CreatedAt') IS NOT NULL ALTER TABLE [Payments] DROP COLUMN [CreatedAt];
            IF COL_LENGTH(N'[Payments]', N'CreatedByUserId') IS NOT NULL ALTER TABLE [Payments] DROP COLUMN [CreatedByUserId];
            IF COL_LENGTH(N'[Payments]', N'Notes') IS NOT NULL ALTER TABLE [Payments] DROP COLUMN [Notes];
            IF COL_LENGTH(N'[Payments]', N'Status') IS NOT NULL ALTER TABLE [Payments] DROP COLUMN [Status];
            IF COL_LENGTH(N'[Payments]', N'UpdatedAt') IS NOT NULL ALTER TABLE [Payments] DROP COLUMN [UpdatedAt];
            """);
    }
}
