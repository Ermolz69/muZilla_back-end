using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace muZilla.Migrations
{
    /// <inheritdoc />
    public partial class SongUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Songs_Images_ImageId",
                table: "Songs");

            migrationBuilder.DropIndex(
                name: "IX_Songs_ImageId",
                table: "Songs");

            migrationBuilder.DropColumn(
                name: "ImageId",
                table: "Songs");

            migrationBuilder.DropColumn(
                name: "LyricsId",
                table: "Songs");

            migrationBuilder.RenameColumn(
                name: "MusicFileId",
                table: "Songs",
                newName: "CoverId");

            migrationBuilder.CreateIndex(
                name: "IX_Songs_CoverId",
                table: "Songs",
                column: "CoverId");

            migrationBuilder.AddForeignKey(
                name: "FK_Songs_Images_CoverId",
                table: "Songs",
                column: "CoverId",
                principalTable: "Images",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Songs_Images_CoverId",
                table: "Songs");

            migrationBuilder.DropIndex(
                name: "IX_Songs_CoverId",
                table: "Songs");

            migrationBuilder.RenameColumn(
                name: "CoverId",
                table: "Songs",
                newName: "MusicFileId");

            migrationBuilder.AddColumn<int>(
                name: "ImageId",
                table: "Songs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LyricsId",
                table: "Songs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Songs_ImageId",
                table: "Songs",
                column: "ImageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Songs_Images_ImageId",
                table: "Songs",
                column: "ImageId",
                principalTable: "Images",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
