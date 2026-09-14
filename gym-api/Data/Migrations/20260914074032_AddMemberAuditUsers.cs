using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMemberAuditUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "Members",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByUserId",
                table: "Members",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Members_CreatedByUserId",
                table: "Members",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Members_UpdatedByUserId",
                table: "Members",
                column: "UpdatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Members_Users_CreatedByUserId",
                table: "Members",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Members_Users_UpdatedByUserId",
                table: "Members",
                column: "UpdatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Members_Users_CreatedByUserId",
                table: "Members");

            migrationBuilder.DropForeignKey(
                name: "FK_Members_Users_UpdatedByUserId",
                table: "Members");

            migrationBuilder.DropIndex(
                name: "IX_Members_CreatedByUserId",
                table: "Members");

            migrationBuilder.DropIndex(
                name: "IX_Members_UpdatedByUserId",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Members");
        }
    }
}
