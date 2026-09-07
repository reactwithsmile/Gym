using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymApi.Data.Migrations;

public partial class RepairTestimonialsTable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            IF OBJECT_ID(N'[Testimonials]', N'U') IS NULL
            BEGIN
                CREATE TABLE [Testimonials] (
                    [Id] int NOT NULL IDENTITY,
                    [CustomerName] nvarchar(200) NOT NULL,
                    [RoleOrDescription] nvarchar(300) NOT NULL,
                    [Review] nvarchar(2000) NOT NULL,
                    [Rating] int NOT NULL,
                    [ImageUrl] nvarchar(max) NOT NULL,
                    [DisplayOrder] int NOT NULL,
                    [IsActive] bit NOT NULL,
                    [CreatedAt] datetime2 NOT NULL,
                    [UpdatedAt] datetime2 NOT NULL,
                    CONSTRAINT [PK_Testimonials] PRIMARY KEY ([Id])
                );

                CREATE INDEX [IX_Testimonials_IsActive_DisplayOrder]
                    ON [Testimonials] ([IsActive], [DisplayOrder]);
            END
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            IF OBJECT_ID(N'[Testimonials]', N'U') IS NOT NULL
                DROP TABLE [Testimonials];
            """);
    }
}
