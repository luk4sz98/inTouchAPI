using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace inTouchAPI.Migrations
{
    public partial class added_column : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreatorId",
                table: "Chats",
                type: "uniqueidentifier",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "Chats");
        }
    }
}
