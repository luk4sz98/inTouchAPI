using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace inTouchAPI.Migrations
{
    public partial class added_delete_cascade_behavior : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatUsers_Users_UserId",
                table: "ChatUsers");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatUsers_Users_UserId",
                table: "ChatUsers",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatUsers_Users_UserId",
                table: "ChatUsers");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatUsers_Users_UserId",
                table: "ChatUsers",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
