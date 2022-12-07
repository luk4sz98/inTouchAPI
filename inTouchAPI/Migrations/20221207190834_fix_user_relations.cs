using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace inTouchAPI.Migrations
{
    public partial class fix_user_relations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserRelations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Relations",
                table: "Relations");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Relations");

            migrationBuilder.AddColumn<string>(
                name: "RequestedByUser",
                table: "Relations",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RequestedToUser",
                table: "Relations",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Relations",
                table: "Relations",
                columns: new[] { "RequestedByUser", "RequestedToUser" });

            migrationBuilder.AddForeignKey(
                name: "FK_Relations_Users_RequestedByUser",
                table: "Relations",
                column: "RequestedByUser",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Relations_Users_RequestedByUser",
                table: "Relations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Relations",
                table: "Relations");

            migrationBuilder.DropColumn(
                name: "RequestedByUser",
                table: "Relations");

            migrationBuilder.DropColumn(
                name: "RequestedToUser",
                table: "Relations");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "Relations",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Relations",
                table: "Relations",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "UserRelations",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RelationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRelations", x => new { x.UserId, x.RelationId });
                    table.ForeignKey(
                        name: "FK_UserRelations_Relations_RelationId",
                        column: x => x.RelationId,
                        principalTable: "Relations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserRelations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserRelations_RelationId",
                table: "UserRelations",
                column: "RelationId");
        }
    }
}
