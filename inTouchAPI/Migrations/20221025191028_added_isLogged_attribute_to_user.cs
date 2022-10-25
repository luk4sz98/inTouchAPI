using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace inTouchAPI.Migrations
{
    public partial class added_isLogged_attribute_to_user : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsLogged",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsLogged",
                table: "Users");
        }
    }
}
