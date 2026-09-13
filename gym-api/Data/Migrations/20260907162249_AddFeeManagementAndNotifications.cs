using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymApi.Data.Migrations;

public partial class AddFeeManagementAndNotifications : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF OBJECT_ID(N'[Members]', N'U') IS NULL
            BEGIN
                CREATE TABLE [Members] (
                    [Id] int NOT NULL IDENTITY,
                    [Name] nvarchar(200) NOT NULL,
                    [Email] nvarchar(256) NOT NULL,
                    [Phone] nvarchar(50) NOT NULL,
                    [IsActive] bit NOT NULL,
                    [CreatedAt] datetime2 NOT NULL,
                    [UpdatedAt] datetime2 NOT NULL,
                    CONSTRAINT [PK_Members] PRIMARY KEY ([Id])
                );
            END;
            IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Members_Email' AND object_id = OBJECT_ID(N'[Members]'))
                CREATE INDEX [IX_Members_Email] ON [Members] ([Email]);

            IF OBJECT_ID(N'[Notifications]', N'U') IS NULL
            BEGIN
                CREATE TABLE [Notifications] (
                    [Id] int NOT NULL IDENTITY,
                    [Title] nvarchar(200) NOT NULL,
                    [Message] nvarchar(2000) NOT NULL,
                    [Type] nvarchar(50) NOT NULL,
                    [DeduplicationKey] nvarchar(300) NULL,
                    [IsRead] bit NOT NULL,
                    [CreatedAt] datetime2 NOT NULL,
                    CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id])
                );
            END;
            IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Notifications_DeduplicationKey' AND object_id = OBJECT_ID(N'[Notifications]'))
                CREATE UNIQUE INDEX [IX_Notifications_DeduplicationKey] ON [Notifications] ([DeduplicationKey]) WHERE [DeduplicationKey] IS NOT NULL;

            IF OBJECT_ID(N'[Memberships]', N'U') IS NULL
            BEGIN
                CREATE TABLE [Memberships] (
                    [Id] int NOT NULL IDENTITY,
                    [MemberId] int NOT NULL,
                    [MembershipPlanId] int NULL,
                    [StartDate] datetime2 NOT NULL,
                    [EndDate] datetime2 NOT NULL,
                    [Status] nvarchar(32) NOT NULL,
                    CONSTRAINT [PK_Memberships] PRIMARY KEY ([Id]),
                    CONSTRAINT [FK_Memberships_Members_MemberId] FOREIGN KEY ([MemberId]) REFERENCES [Members] ([Id]) ON DELETE CASCADE,
                    CONSTRAINT [FK_Memberships_MembershipPlans_MembershipPlanId] FOREIGN KEY ([MembershipPlanId]) REFERENCES [MembershipPlans] ([Id]) ON DELETE SET NULL
                );
            END;
            IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Memberships_EndDate_Status' AND object_id = OBJECT_ID(N'[Memberships]'))
                CREATE INDEX [IX_Memberships_EndDate_Status] ON [Memberships] ([EndDate], [Status]);

            IF OBJECT_ID(N'[Payments]', N'U') IS NULL
            BEGIN
                CREATE TABLE [Payments] (
                    [Id] int NOT NULL IDENTITY,
                    [MemberId] int NOT NULL,
                    [MembershipId] int NULL,
                    [Amount] decimal(18,2) NOT NULL,
                    [PaidAt] datetime2 NOT NULL,
                    [Method] nvarchar(50) NOT NULL,
                    [Reference] nvarchar(200) NOT NULL,
                    [Status] nvarchar(32) NOT NULL,
                    [Notes] nvarchar(1000) NOT NULL,
                    [CreatedByUserId] int NULL,
                    [CreatedAt] datetime2 NOT NULL,
                    [UpdatedAt] datetime2 NOT NULL,
                    CONSTRAINT [PK_Payments] PRIMARY KEY ([Id]),
                    CONSTRAINT [FK_Payments_Members_MemberId] FOREIGN KEY ([MemberId]) REFERENCES [Members] ([Id]) ON DELETE NO ACTION,
                    CONSTRAINT [FK_Payments_Memberships_MembershipId] FOREIGN KEY ([MembershipId]) REFERENCES [Memberships] ([Id]) ON DELETE SET NULL,
                    CONSTRAINT [FK_Payments_Users_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [Users] ([Id]) ON DELETE SET NULL
                );
            END;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF OBJECT_ID(N'[Payments]', N'U') IS NOT NULL DROP TABLE [Payments];
            IF OBJECT_ID(N'[Memberships]', N'U') IS NOT NULL DROP TABLE [Memberships];
            IF OBJECT_ID(N'[Notifications]', N'U') IS NOT NULL DROP TABLE [Notifications];
            IF OBJECT_ID(N'[Members]', N'U') IS NOT NULL DROP TABLE [Members];
            """);
    }
}
