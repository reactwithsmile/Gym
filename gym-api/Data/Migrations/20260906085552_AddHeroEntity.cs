using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddHeroEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Heroes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Heading = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    PrimaryButtonText = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PrimaryButtonLink = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SecondaryButtonText = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SecondaryButtonLink = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Heroes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Heroes_IsActive",
                table: "Heroes",
                column: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Heroes");
        }
    }
}
