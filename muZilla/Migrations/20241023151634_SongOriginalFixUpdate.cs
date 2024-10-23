using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace muZilla.Migrations
{
    /// <inheritdoc />
    public partial class SongOriginalFixUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Songs_Songs_OriginalId1",
                table: "Songs");

            migrationBuilder.DropIndex(
                name: "IX_Songs_OriginalId1",
                table: "Songs");

            migrationBuilder.DropColumn(
                name: "OriginalId1",
                table: "Songs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OriginalId1",
                table: "Songs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Songs_OriginalId1",
                table: "Songs",
                column: "OriginalId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Songs_Songs_OriginalId1",
                table: "Songs",
                column: "OriginalId1",
                principalTable: "Songs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
