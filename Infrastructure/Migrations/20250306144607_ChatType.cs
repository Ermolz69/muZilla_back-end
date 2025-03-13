using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace muZilla.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChatType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatorId",
                table: "Chats",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Chats",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Chats",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Chats_CreatorId",
                table: "Chats",
                column: "CreatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chats_Users_CreatorId",
                table: "Chats",
                column: "CreatorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chats_Users_CreatorId",
                table: "Chats");

            migrationBuilder.DropIndex(
                name: "IX_Chats_CreatorId",
                table: "Chats");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "Chats");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Chats");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Chats");
        }
    }
}
