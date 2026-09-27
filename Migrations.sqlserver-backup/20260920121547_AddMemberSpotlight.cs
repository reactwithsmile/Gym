using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMemberSpotlight : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MemberSpotlights",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MemberName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Quote = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Story = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DurationText = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AchievementText = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsFeatured = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberSpotlights", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MemberSpotlights_IsActive_IsFeatured_DisplayOrder",
                table: "MemberSpotlights",
                columns: new[] { "IsActive", "IsFeatured", "DisplayOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MemberSpotlights");
        }
    }
}
